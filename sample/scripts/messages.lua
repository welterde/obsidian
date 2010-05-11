function PlayerLogin(player,name)
  player.ReadyEvent:Add(PlayerReady)
  player.DisconnectedEvent:Add(PlayerDisconnected)
end

function PlayerReady(player)
  local count = server.Players.Count-1;
  local players = count.." players"
  if count == 0 then
    players = "No players"
  else
    if count == 1 then
      players = "1 player"
    end
  end
  Message("&a[+]&e "..player.Group.Prefix..player.Name..player.Group.Postfix.."&e joined the game."):Send(server)
  Message("&eWelcome, "..player.Name..". "..players.." online."):Send(player)
end

function PlayerDisconnected(player,message)
  if message == nil then
    Message("&c[-]&e "..player.Group.Prefix..player.Name..player.Group.Postfix.."&e left the game."):Send(server)
  else
    Message("&c[-]&e "..player.Group.Prefix..player.Name..player.Group.Postfix.."&e left the game ("..message..")."):Send(server)
  end
end

server.PlayerLoginEvent:Add(PlayerLogin)