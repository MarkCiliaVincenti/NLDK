﻿
using AsyncKeyedLock;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBXplorer;
using NBXplorer.Models;
using NLDK;
using nldksample.LDK;
using org.ldk.structs;
using UInt128 = org.ldk.util.UInt128;


var builder = WebApplication.CreateBuilder(args);


var nbxNetworkProvider = new NBXplorerNetworkProvider(ChainName.Regtest);

builder.Configuration.AddEnvironmentVariables("NLDK_");
builder.Services
    .Configure<NLDKOptions>(builder.Configuration)
    .AddHostedService<MigratonHostedService>()
    .AddHostedService<NBXListener>(provider => provider.GetRequiredService<NBXListener>())
    .AddSingleton<AsyncKeyedLocker<string>>()
    .AddSingleton<WalletService>()
    .AddLDK()
    .AddSingleton<NBXListener>()
    .AddSingleton<Network>(provider => provider.GetRequiredService<ExplorerClient>().Network.NBitcoinNetwork)
    .AddSingleton<ExplorerClient>(sp => 
        new ExplorerClient(nbxNetworkProvider.GetFromCryptoCode("BTC"), new Uri(sp.GetRequiredService<IOptions<NLDKOptions>>().Value.NBXplorerConnection)))
    .AddDbContextFactory<WalletContext>((provider, optionsBuilder) => optionsBuilder.UseSqlite(provider.GetRequiredService<IOptions<NLDKOptions>>().Value.ConnectionString));

var app = builder.Build();
_ = app.RunAsync();



async Task<string> CreateWallet(IServiceProvider serviceProvider)
{
    var explorerClient = serviceProvider.GetRequiredService<ExplorerClient>();
    var walletService = serviceProvider.GetRequiredService<WalletService>();

    var genWallet = await explorerClient.GenerateWalletAsync();
    var hash = await explorerClient.RPCClient.GetBestBlockHashAsync();
    var ts = new DerivationSchemeTrackedSource(genWallet.DerivationScheme);
    var idx = genWallet.GetMnemonic().DeriveExtKey().GetPublicKey().GetHDFingerPrint().ToString();
    var wts = new GroupTrackedSource(idx);
    await explorerClient.TrackAsync(wts);
    await explorerClient.AddGroupChildrenAsync(wts.GroupId, new[]
    {
        new GroupChild()
        {
            TrackedSource = ts.ToString(),
            CryptoCode = "BTC"
        }
    });
    return await walletService.Create(genWallet.GetMnemonic(), null, genWallet.AccountKeyPath.KeyPath + "/*",
        hash.ToString(), new[] {ts.ToString(), wts.ToString()});
}
var explorerClient = app.Services.GetRequiredService<ExplorerClient>();
var walletService = app.Services.GetRequiredService<WalletService>();
var nodeManager = app.Services.GetRequiredService<LDKNodeManager>();

var wallet1 = await CreateWallet(app.Services);
var wallet2 = await CreateWallet(app.Services);

var wallet1Node = await nodeManager.GetLDKNodeForWallet(wallet1);
var wallet2Node = await nodeManager.GetLDKNodeForWallet(wallet2);

var wallet1Script = await walletService.DeriveScript(wallet1);
var wallet2Script = await walletService.DeriveScript(wallet1);
await explorerClient.RPCClient.GenerateAsync(2);
await explorerClient.RPCClient.SendToAddressAsync(wallet1Script.ToScript(), Money.Coins(1));
await explorerClient.RPCClient.SendToAddressAsync(wallet2Script.ToScript(), Money.Coins(2));
await explorerClient.RPCClient.GenerateAsync(1);

while (wallet1Node.NodeInfo is null || wallet2Node.NodeInfo is null)
{
    await Task.Delay(100);
}

var wallet1PeerHandler = wallet1Node.ServiceProvider.GetRequiredService<LDKPeerHandler>();
var wallet2PeerHandler = wallet2Node.ServiceProvider.GetRequiredService<LDKPeerHandler>();
LDKTcpDescriptor? wallet1Peer = null;

while (wallet1Peer is null)
{
    wallet1Peer = await wallet1PeerHandler.ConnectAsync(wallet2Node.NodeInfo);
}
var wallet1ChannelManager= wallet1Node.ServiceProvider.GetRequiredService<ChannelManager>();
var wallet1UserConfig= wallet1Node.ServiceProvider.GetRequiredService<UserConfig>();
var userChannelId = new UInt128(RandomUtils.GetBytes(16));
var channelResult = wallet1ChannelManager.create_channel(
    wallet2Node.NodeId.ToBytes(), 
    Money.Coins(0.5m).Satoshi, 
    0,
    userChannelId, 
    wallet1UserConfig);

while(wallet1ChannelManager.list_channels().Length == 0)
{
    await Task.Delay(100);
}

var channels = wallet1ChannelManager.list_channels();

var channel = channels[0];


