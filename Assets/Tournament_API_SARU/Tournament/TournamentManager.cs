using System;
using System.Collections.Generic;
using UnityEngine;

public class TournamentManager : MonoBehaviour
{

    public Action<string> OnFetchTournamentDataFailed;
    public Action<List<TournamentModel>> OnUpdateTournamentDetails;


    public Action<List<BetHistoryModel>> OnUpdateBetHistory;
    public Dictionary<string, Action<TournamentModel>> OnUpdateSingleTournamentUpdate = new Dictionary<string, Action<TournamentModel>>();
    public static TournamentManager instance;
    

    public void LoadAllTournamentData(string user_id)
    {

        bool isLoadingTournament = false;
        if (isLoadingTournament)
        {
            Debug.Log("request already in progress.");
            return;
        }
        Debug.Log("request.");
        isLoadingTournament = true;
        userID = user_id;
        TimerHelper.instance.CronForEveryOneMin -= tournament.ValidateTournament;
        Debug.Log("unsubscribe ValidateTournament");

        tournament = new TournamentController();
        OnUpdateSingleTournamentUpdate = new Dictionary<string, Action<TournamentModel>>();
        LoadTournamentData();
    }

    string userID;



    [SerializeField]
    private TournamentController tournament = new TournamentController();
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        FetchTour();
    }
    private void OnDisable()
    {
        TimerHelper.instance.CronForEveryOneMin -= tournament.ValidateTournament;

        Debug.Log("unsubscribe ValidateTournament");

    }

    void FetchTour()
    {

        LoadAllTournamentData("cb5d2c8a-9217-4566-9c4c-2e57b7f03b93");
    }
    bool isLoadingTournament = false;
    int trycount = 0;
    
    void ResetFetch()
    {
        trycount = 0;
        isLoadingTournament = false;
    }
    void LoadTournamentData()
    {
        tournament.FetchTournamentData(userID, (success, data) =>
        {
            if(success)
            {
                TimerHelper.instance.CronForEveryOneMin += tournament.ValidateTournament;
                tournament.userID = userID;
                Debug.Log("subscribe ValidateTournament");
                ResetFetch();
                OnUpdateBetHistory?.Invoke(tournament.betHistory.betHistories);
                OnUpdateTournamentDetails?.Invoke(tournament.TournamentList);

                Debug.Log("invoke tournamnet changes :::  OnUpdateBetHistory");
                Debug.Log("invoke tournamnet changes :::  OnUpdateTournamentDetails");

            }
            else
            {
                Debug.Log("error while fetch data");
                trycount += 1;
                if(trycount > 3)
                {
                    Invoke(nameof(LoadTournamentData),2);
                }
                else
                {
                    OnFetchTournamentDataFailed?.Invoke(data);

                    Debug.Log("invoke tournamnet changes :::  OnFetchTournamentDataFailed");
                    ResetFetch();
                }
            }
        });

    }


}
