using System.Collections.Generic;
using Libplanet.Action;
using Libplanet.Unity;
using UnityEngine;

namespace Scripts.Actions
{
    [ActionType("game_log")]
    public class GameLogAction : ActionBase
    {
        public string PlayerName { get; set; }
        public string ConversationLog { get; set; }
        public string GameResult { get; set; }
        public string Timestamp { get; set; }

        public GameLogAction()
        {
        }

        public GameLogAction(string playerName, string conversationLog,
                                 string gameResult, string timestamp)
        {
            PlayerName = playerName;
            ConversationLog = conversationLog;
            GameResult = gameResult;
            Timestamp = timestamp;
        }

        public override Bencodex.Types.IValue PlainValue =>
            new Bencodex.Types.Dictionary(new Dictionary<Bencodex.Types.IKey, Bencodex.Types.IValue>
            {
                [(Bencodex.Types.Text)"player_name"] = (Bencodex.Types.Text)(PlayerName ?? ""),
                [(Bencodex.Types.Text)"conversation_log"] = (Bencodex.Types.Text)(ConversationLog ?? ""),
                [(Bencodex.Types.Text)"game_result"] = (Bencodex.Types.Text)(GameResult ?? ""),
                [(Bencodex.Types.Text)"timestamp"] = (Bencodex.Types.Text)(Timestamp ?? "")
            });

        public override void LoadPlainValue(Bencodex.Types.IValue plainValue)
        {
            if (plainValue is Bencodex.Types.Dictionary dict)
            {
                PlayerName = dict.TryGetValue((Bencodex.Types.Text)"player_name", out var playerNameValue) && playerNameValue is Bencodex.Types.Text playerNameText ? playerNameText.Value : "";
                ConversationLog = dict.TryGetValue((Bencodex.Types.Text)"conversation_log", out var conversationValue) && conversationValue is Bencodex.Types.Text conversationText ? conversationText.Value : "";
                GameResult = dict.TryGetValue((Bencodex.Types.Text)"game_result", out var gameResultValue) && gameResultValue is Bencodex.Types.Text gameResultText ? gameResultText.Value : "";
                Timestamp = dict.TryGetValue((Bencodex.Types.Text)"timestamp", out var timestampValue) && timestampValue is Bencodex.Types.Text timestampText ? timestampText.Value : "";
            }
        }

        public override IAccountStateDelta Execute(IActionContext context)
        {
            IAccountStateDelta states = context.PreviousStates;

            var oldState = states.GetState(context.Signer);
            string oldLog = oldState is Bencodex.Types.Text t ? t.Value : "";

            var newEntry = $"{Timestamp}|{PlayerName}|{GameResult}|{ConversationLog}";
            string combinedLog = string.IsNullOrEmpty(oldLog) ? newEntry : $"{newEntry}‖{oldLog}";

            return states.SetState(context.Signer, (Bencodex.Types.Text)combinedLog);
        }
    }
}
