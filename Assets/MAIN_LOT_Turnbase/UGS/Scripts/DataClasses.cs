using System;
using System.Globalization;
using UnityEngine.Serialization;

namespace JumpeeIsland
{
    [Serializable]
    public class InboxMessage
    {
        // The messageId, which also serves as the Remote Config key for a given message.
        public string messageId;

        // MessageInfo is all the universal message data stored in Remote Config
        public MessageInfo messageInfo;

        // MessageMetadata is all the player-specific instance data for a given message, stored in Cloud Save.
        public MessageMetadata metadata;

        public MailType mailType;
        
        // MessageBattleData is about the battle when other player invade your land
        public MessageBattleData battleData;

        public InboxMessage(string messageId = "", MessageInfo messageInfo = null, MessageMetadata metadata = null, MessageBattleData battleData = null)
        {
            this.messageId = messageId;
            this.messageInfo = messageInfo;
            this.metadata = metadata;
            this.battleData = battleData;
        }
    }

    [Serializable]
    public class MessageInfo
    {
        public string title;
        public string content;
        public string attachment;
        public string expiration;
    }

    [Serializable]
    public class MessageMetadata
    {
        public string expirationDate;
        public bool isRead;
        public bool hasUnclaimedAttachment;

        public MessageMetadata(TimeSpan expiration, bool hasUnclaimedAttachment = false)
        {
            var expirationDateTime = DateTime.Now.Add(expiration);

            // "s" format for DateTime results in a string like "2008-10-31T17:04:32"
            expirationDate = expirationDateTime.ToString("s", CultureInfo.GetCultureInfo("en-US"));
            this.hasUnclaimedAttachment = hasUnclaimedAttachment;
            isRead = false;
        }
    }

    [Serializable]
    public class MessageBattleData
    {
        public BattleRecord battleRecord;
    }

    public class ItemAndAmountSpec
    {
        public string id;
        public int amount;

        public ItemAndAmountSpec(string id, int amount)
        {
            this.id = id;
            this.amount = amount;
        }

        public override string ToString()
        {
            return $"{id}:{amount}";
        }
    }

    public enum MailType
    {
        NONE,
        INFO,
        BATTLE
    }
}