﻿//  __  _  __    __   ___ __  ___ ___  
// |  \| |/__\ /' _/ / _//__\| _ \ __| 
// | | ' | \/ |`._`.| \_| \/ | v / _|  
// |_|\__|\__/ |___/ \__/\__/|_|_\___| 
// 
// Copyright (C) 2018 - NosCore
// 
// NosCore is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using NosCore.GameObject.ComponentEntities.Extensions;
using NosCore.GameObject.Networking.ClientSession;
using NosCore.GameObject.Networking.Group;
using NosCore.Packets.ClientPackets;
using NosCore.Packets.ServerPackets;
using NosCore.Shared.Enumerations;
using NosCore.Shared.I18N;
using System;

namespace NosCore.GameObject.Services.MapItemBuilder.Handlers
{
    public class GoldDropHandler : IHandler<MapItem, Tuple<MapItem, GetPacket>>
    {
        public bool Condition(MapItem item) => item.VNum == 1046;

        public void Execute(RequestData<Tuple<MapItem, GetPacket>> requestData)
        {
            // handle gold drop
            var maxGold = requestData.ClientSession.WorldConfiguration.MaxGoldAmount;
            if (requestData.ClientSession.Character.Gold + requestData.Data.Item1.Amount <= maxGold)
            {
                if (requestData.Data.Item2.PickerType == PickerType.Mate)
                {
                    requestData.ClientSession.SendPacket(
                        requestData.ClientSession.Character.GenerateIcon(1, requestData.Data.Item1.VNum));
                }

                requestData.ClientSession.Character.Gold += requestData.Data.Item1.Amount;
                requestData.ClientSession.SendPacket(requestData.ClientSession.Character.GenerateSay(
                    $"{Language.Instance.GetMessageFromKey(LanguageKey.ITEM_ACQUIRED, requestData.ClientSession.Account.Language)}" +
                    $": {requestData.Data.Item1.ItemInstance.Item.Name} x {requestData.Data.Item1.Amount}",
                    SayColorType.Green));
            }
            else
            {
                requestData.ClientSession.Character.Gold = maxGold;
                requestData.ClientSession.SendPacket(new MsgPacket
                {
                    Message = Language.Instance.GetMessageFromKey(LanguageKey.MAX_GOLD,
                        requestData.ClientSession.Account.Language),
                    Type = 0
                });
            }

            requestData.ClientSession.SendPacket(requestData.ClientSession.Character.GenerateGold());
            requestData.ClientSession.Character.MapInstance.MapItems.TryRemove(requestData.Data.Item1.VisualId, out _);
            requestData.ClientSession.Character.MapInstance.Sessions.SendPacket(
                requestData.ClientSession.Character.GenerateGet(requestData.Data.Item1.VisualId));
        }
    }
}