using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TournamentManager : MonoBehaviour
{

    public Action<string> OnFetchTournamentDataFailed;
    public Action<List<TournamentModel>> OnUpdateTournamentDetails;


    public Action<List<BetHistoryModel>> OnUpdateBetHistory;
    public Dictionary<string, Action<TournamentModel>> OnUpdateSingleTournamentUpdate = new Dictionary<string, Action<TournamentModel>>();
    public static TournamentManager instance;


    public void AddTicket(string tournament_id, string round_id, string session_token, TicketList ticket_data, int amount, string userToken, string currency, string meta_data, Action<bool, string> action ,string user_id)
    {
        tournament.BuyTicketsForTournament(tournament_id,round_id,session_token,ticket_data,amount,userToken,currency,meta_data,action,user_id);
    }
    public void CancelTicket(string ticket_id, string tournament_id, string session_token, string userToken, string currency, Action<bool, string> action, string user_id)
    {
        tournament.CancelTournamentTicket(ticket_id, tournament_id, session_token, userToken, currency, action, user_id);
    }
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
            if (success)
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
                if (trycount > 3)
                {
                    Invoke(nameof(LoadTournamentData), 2);
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


    [ContextMenu("load bet")]
    void LoadBetData()
    {
        tournament.betHistory.GetTournamentBetList(tournament.APIURL,userID, (success) =>
        {
            if (success)
            {
            
                OnUpdateBetHistory?.Invoke(tournament.betHistory.betHistories);
              
                Debug.Log("invoke tournamnet changes :::  OnUpdateBetHistory");
           
            }

        });

    }




    [ContextMenu("Test This ===> ")]
    public void Test()
    {
        tournament.TestCase();
    }

    [SerializeField]
    public TournamentModel TournamentFound = new TournamentModel();
    public string findTournamet;
    [ContextMenu("Find Tournament")]
    public void FindFournament()
    {
        TournamentFound = tournament.TournamentList.Find(x => x.id == findTournamet);
    }
    [ContextMenu("Load Live")]
    public void LoadLive()
    {
        TournamentFound = tournament.TournamentList.Find(x => x.round_data[0].isLive);
    }
}
