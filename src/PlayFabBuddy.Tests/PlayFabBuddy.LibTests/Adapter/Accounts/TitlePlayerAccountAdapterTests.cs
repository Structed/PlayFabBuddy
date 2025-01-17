﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlayFabBuddy.Lib.Adapter.Accounts;

namespace PlayFabBuddy.Lib.Tests.Adapter.Accounts;

[TestClass]
public class TitlePlayerAccountAdapterTests
{
    [TestMethod]
    public void TitlePlayerAccountEntityTest()
    {
        var guid = Guid.NewGuid().ToString();

        var account = new TitlePlayerAccountAdapter(guid);

        Assert.AreEqual(account.PlayerAccount.Id, guid);
    }

    [TestMethod]
    public void TitlePlayerAccountEntityTest1()
    {
        var guid = Guid.NewGuid().ToString();

        var masterAccount = new MasterPlayerAccountAdapter(guid);

        var titleAccount = new TitlePlayerAccountAdapter(guid, masterAccount.MainAccount);

        Assert.IsNotNull(masterAccount.MainAccount.PlayerAccounts);
        Assert.IsTrue(masterAccount.MainAccount.PlayerAccounts.Count == 1);
        Assert.AreEqual(titleAccount.PlayerAccount, masterAccount.MainAccount.PlayerAccounts.First());
    }

    [TestMethod]
    public void AssignMasterAccountTest()
    {
        var guid = Guid.NewGuid().ToString();

        var masterAccount = new MasterPlayerAccountAdapter(guid).MainAccount;
        var newMasterAccount = new MasterPlayerAccountAdapter(Guid.NewGuid().ToString()).MainAccount;

        var titleAccount = new TitlePlayerAccountAdapter(guid, masterAccount);
        titleAccount.AssignMasterAccount(newMasterAccount);

        Assert.IsNotNull(masterAccount.PlayerAccounts);
        Assert.IsNotNull(newMasterAccount.PlayerAccounts);

        Assert.IsTrue(newMasterAccount.PlayerAccounts.Count == 1);
        Assert.AreEqual(titleAccount.PlayerAccount, newMasterAccount.PlayerAccounts.First());
    }
}