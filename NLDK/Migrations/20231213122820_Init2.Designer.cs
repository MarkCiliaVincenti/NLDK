﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NLDK;

#nullable disable

namespace NLDK.Migrations
{
    [DbContext(typeof(WalletContext))]
    [Migration("20231213122820_Init2")]
    partial class Init2
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("NLDK.Channel", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("WalletId")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<string>("FundingTransactionHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("FundingTransactionOutputIndex")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id", "WalletId");

                    b.HasIndex("WalletId");

                    b.HasIndex("FundingTransactionHash", "FundingTransactionOutputIndex")
                        .IsUnique();

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("NLDK.Coin", b =>
                {
                    b.Property<string>("FundingTransactionHash")
                        .HasColumnType("TEXT");

                    b.Property<int>("FundingTransactionOutputIndex")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ScriptId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SpendingTransactionHash")
                        .HasColumnType("TEXT");

                    b.Property<int?>("SpendingTransactionInputIndex")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("FundingTransactionHash", "FundingTransactionOutputIndex");

                    b.HasIndex("ScriptId");

                    b.ToTable("Coins");
                });

            modelBuilder.Entity("NLDK.Script", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Scripts");
                });

            modelBuilder.Entity("NLDK.Transaction", b =>
                {
                    b.Property<string>("Hash")
                        .HasColumnType("TEXT");

                    b.Property<string>("BlockHash")
                        .HasColumnType("TEXT");

                    b.HasKey("Hash");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("NLDK.TransactionScript", b =>
                {
                    b.Property<string>("ScriptId")
                        .HasColumnType("TEXT");

                    b.Property<string>("TransactionHash")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Spent")
                        .HasColumnType("INTEGER");

                    b.HasKey("ScriptId", "TransactionHash");

                    b.HasIndex("TransactionHash");

                    b.ToTable("TransactionScripts");
                });

            modelBuilder.Entity("NLDK.Wallet", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("AliasWalletName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CreationBlockHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("DerivationPath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<uint>("LastDerivationIndex")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Mnemonic")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Wallets");
                });

            modelBuilder.Entity("NLDK.WalletScript", b =>
                {
                    b.Property<string>("WalletId")
                        .HasColumnType("TEXT");

                    b.Property<string>("ScriptId")
                        .HasColumnType("TEXT");

                    b.Property<string>("DerivationPath")
                        .HasColumnType("TEXT");

                    b.HasKey("WalletId", "ScriptId");

                    b.HasIndex("ScriptId");

                    b.ToTable("WalletScripts");
                });

            modelBuilder.Entity("NLDK.Channel", b =>
                {
                    b.HasOne("NLDK.Wallet", "Wallet")
                        .WithMany("Channels")
                        .HasForeignKey("WalletId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NLDK.Coin", "Coin")
                        .WithOne("Channel")
                        .HasForeignKey("NLDK.Channel", "FundingTransactionHash", "FundingTransactionOutputIndex")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coin");

                    b.Navigation("Wallet");
                });

            modelBuilder.Entity("NLDK.Coin", b =>
                {
                    b.HasOne("NLDK.Script", "Script")
                        .WithMany("Coins")
                        .HasForeignKey("ScriptId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Script");
                });

            modelBuilder.Entity("NLDK.TransactionScript", b =>
                {
                    b.HasOne("NLDK.Script", "Script")
                        .WithMany("TransactionScripts")
                        .HasForeignKey("ScriptId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NLDK.Transaction", "Transaction")
                        .WithMany("TransactionScripts")
                        .HasForeignKey("TransactionHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Script");

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("NLDK.WalletScript", b =>
                {
                    b.HasOne("NLDK.Script", "Script")
                        .WithMany("WalletScripts")
                        .HasForeignKey("ScriptId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NLDK.Wallet", "Wallet")
                        .WithMany("Scripts")
                        .HasForeignKey("WalletId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Script");

                    b.Navigation("Wallet");
                });

            modelBuilder.Entity("NLDK.Coin", b =>
                {
                    b.Navigation("Channel");
                });

            modelBuilder.Entity("NLDK.Script", b =>
                {
                    b.Navigation("Coins");

                    b.Navigation("TransactionScripts");

                    b.Navigation("WalletScripts");
                });

            modelBuilder.Entity("NLDK.Transaction", b =>
                {
                    b.Navigation("TransactionScripts");
                });

            modelBuilder.Entity("NLDK.Wallet", b =>
                {
                    b.Navigation("Channels");

                    b.Navigation("Scripts");
                });
#pragma warning restore 612, 618
        }
    }
}