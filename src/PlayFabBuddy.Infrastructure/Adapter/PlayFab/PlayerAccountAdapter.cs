﻿using PlayFab;
using PlayFab.AdminModels;
using PlayFab.ClientModels;
using PlayFabBuddy.Lib.Adapter.Accounts;
using PlayFabBuddy.Lib.Entities.Accounts;
using PlayFabBuddy.Lib.Interfaces.Adapter;

namespace PlayFabBuddy.Infrastructure.Adapter.PlayFab;

public class PlayerAccountAdapter : IPlayerAccountAdapter
{
    public async Task Delete(MasterPlayerAccountEntity account)
    {
        var playedTitleList = await GetPlayedTitleList(account);
        if (playedTitleList.MoreThanOne())
        {
            throw new Exception($"Master PlayerAccount ID \"{account.Id}\" has more than one Title");
        }
        
        var request = new DeleteMasterPlayerAccountRequest
        {
            PlayFabId = account.Id
        };

        await PlayFabAdminAPI.DeleteMasterPlayerAccountAsync(request);
    }

    public async Task<PlayedTitlesListEntity> GetPlayedTitleList(MasterPlayerAccountEntity account)
    {
        var request = new GetPlayedTitleListRequest { PlayFabId = account.Id };

        var response =await PlayFabAdminAPI.GetPlayedTitleListAsync(request);
        return new PlayedTitlesListEntity(response.Result.TitleIds.ToArray());
    }

    public async Task<MasterPlayerAccountEntity> LoginWithCustomId(string customId)
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = customId,
            CreateAccount = true
        };

        /*
         * Result:
         *  AuthenticationContext: 
         *      EntityId = Title Player Account Id
         *      PlayFabId = Master Player account Id
         *  
         */
        var loginResult = await PlayFabClientAPI.LoginWithCustomIDAsync(request);
        var mainAccount = new MasterPlayerAccountAdapter(loginResult.Result.AuthenticationContext.PlayFabId);
        var titleAccount =
            new TitlePlayerAccountAdapter(loginResult.Result.AuthenticationContext.EntityId, mainAccount.MainAccount);

        return mainAccount.MainAccount;
    }
}
