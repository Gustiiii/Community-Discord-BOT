﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CommunityBot.Features.Economy;
using CommunityBot.Features.GlobalAccounts;
using Discord;
using Discord.Commands;

namespace CommunityBot.Modules
{
    public class Economy : ModuleBase<SocketCommandContext>
    {
        [Command("Daily")]
        [Alias("GetDaily", "ClaimDaily")]
        public async Task GetDaily()
        {
            var result = Daily.GetDaily(Context.User.Id);
            switch (result)
            {
                case Daily.DailyResult.AlreadyRecieved:
                    await ReplyAsync($"You already got your daily, {Context.User.Mention}.");
                    break;
                case Daily.DailyResult.Success:
                    await ReplyAsync($"Here's {Constants.DailyMuiniesGain} miunies, {Context.User.Mention}! Just for you...");
                    break;
            }
        }

        [Command("Miunies")]
        [Alias("Cash", "Money")]
        public async Task CheckMiunies()
        {
            var account = GlobalUserAccounts.GetUserAccount(Context.User.Id);
            await ReplyAsync(GetMiuniesReport(account.Miunies, Context.User.Mention));
        }

        [Command("Miunies")]
        [Alias("Cash", "Money")]
        public async Task CheckMiuniesOther(IGuildUser target)
        {
            var account = GlobalUserAccounts.GetUserAccount(target.Id);
            await ReplyAsync(GetMiuniesReport(account.Miunies, target.Mention));
        }

        [Command("Transfer")]
        [Remarks("Transferrs specified amount of your Minuies to the mentioned person.")]
        [Alias("Give", "Gift")]
        public async Task TransferMinuies(IGuildUser target, ulong amount)
        {
            if (Context.User.Id == target.Id)
            {
                await ReplyAsync($":negative_squared_cross_mark: You can\'t gift yourself...\n**And you KNOW it!**");
                return;
            }
            var transferSource = GlobalUserAccounts.GetUserAccount(Context.User.Id);
            if (transferSource.Miunies < amount)
            {
                await ReplyAsync($":negative_squared_cross_mark: You don\'t have that much Minuies! You only have {transferSource.Miunies}.");
                return;
            }
            var transferTarget = GlobalUserAccounts.GetUserAccount(target.Id);
            transferSource.Miunies -= amount;
            transferTarget.Miunies += amount;
            GlobalUserAccounts.SaveAccounts(transferSource.Id, transferTarget.Id);
            await ReplyAsync($" :white_check_mark: {Context.User.Username} has given {target.Username} {amount} Minuies!");
        }

        public string GetMiuniesReport(ulong miunies, string mention)
        {
            return $"{mention} has **{miunies} miunies**! {GetMiuniesCountReaction(miunies, mention)} \n\nDid you know?\n`{Global.GetRandomDidYouKnow()}`";
        }

        [Command("newslot")]
        [Alias("newslots")]
        public async Task NewSlot(int amount = 0)
        {
            Global.slot = new Slot(amount);
            await ReplyAsync("A new slotmachine got generated! Good luck with this puppy!");
        }

        [Command("slots")]
        [Alias("slot")]
        public async Task SpinSlot(uint amount)
        {
            if (amount < 1)
            {
                await ReplyAsync($"You can' spin for that amount of Miunies.\nAND YOU KNOW IT!");
                return;
            }
            var account = GlobalUserAccounts.GetUserAccount(Context.User.Id);
            if (account.Miunies < amount)
            {
                await ReplyAsync($"Sorry but it seems like you don't have enough Minuies... You only have {account.Miunies}.");
                return;
            }

            account.Miunies -= amount;
            GlobalUserAccounts.SaveAccounts(Context.User.Id);

            string slotEmojis = Global.slot.Spin();
            var payoutAndFlavour = Global.slot.GetPayoutAndFlavourText(amount);

            if (payoutAndFlavour.Item1 > 0)
            {
                account.Miunies += payoutAndFlavour.Item1;
                GlobalUserAccounts.SaveAccounts();
            }            

            IUserMessage msg = await ReplyAsync(slotEmojis);
            await Task.Delay(1000);
            await ReplyAsync(payoutAndFlavour.Item2);
        }

        [Command("showslots")]
        [Alias("showslot")]
        public async Task ShowSlot()
        {
            await ReplyAsync(String.Join("\n", Global.slot.GetCylinderEmojis(true)));
        }

        private string GetMiuniesCountReaction(ulong value, string mention)
        {
            if (value > 100000)
            {
                return $"Holy shit, {mention}! You're either cheating or you're really dedicated.";
            }
            else if (value > 50000)
            {
                return $"Damn, you must be here often, {mention}. Do you have a crush on me or something?";
            }
            else if (value > 20000)
            {
                return $"That's enough to buy a house... In Miunie land... \n\nIt's a real place, shut up, {mention}!";
            }
            else if (value > 10000)
            {
                return $"{mention} is kinda getting rich. Do we rob them or what?";
            }
            else if (value > 5000)
            {
                return $"Is it just me or is {mention} taking this economy a little too seriously?";
            }
            else if (value > 2500)
            {
                return $"Great, {mention}! Now you can give all those miunies to your superior mistress, ME.";
            }
            else if (value > 1100)
            {
                return $"{mention} is showing their wealth on the internet again.";
            }
            else if (value > 800)
            {
                return $"Alright, {mention}. Put the miunies in the back and nobody gets hurt.";
            }
            else if (value > 550)
            {
                return $"I like how {mention} think that's impressive.";
            }
            else if (value > 200)
            {
                return $"Outch, {mention}! If I knew that is all you've got, I would just DM you the amount. Embarrassing!";
            }
            else if (value == 0)
            {
                return $"Yea, {mention} is broke. What a surprise.";
            }

            return $"The whole concept of miunies is fake. I hope you know that";
        }
    }
}
