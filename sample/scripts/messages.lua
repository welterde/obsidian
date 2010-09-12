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
  player.ChatEvent:Add(PlayerChat)
end

function PlayerDisconnected(player,message)
  if message == nil then
    Message("&c[-]&e "..player.Group.Prefix..player.Name..player.Group.Postfix.."&e left the game."):Send(server)
  else
    Message("&c[-]&e "..player.Group.Prefix..player.Name..player.Group.Postfix.."&e left the game ("..message..")."):Send(server)
  end
end

function PlayerChat(player,message)
  if message == "" then return end
  if message:sub(1,1) == "@" then
    local index = message:find(" ",3)
    if index == nil then
      Message("&eSyntax: @<player> <message>"):Send(player)
      return
    end
    local name = message:sub(2,index-1)
    if name:sub(1,1) == " " then
      name = name:sub(2)
    end
    local target = FindPlayer(name)
    if target == nil then
      Message("&eThere is no player '"..name.."'."):Send(player)
      return
    end
    if target == player then
      Message("&eTrying to talk to yourself, huh?"):Send(player)
      return
    end
    message = message:sub(index+1)
    server:Log(player.Name.." @ "..target.Name..": "..message)
    Message("&9[->] "..target.Group.Prefix..target.Name..target.Group.Postfix..": &9"..message):Send(player)
    Message("&e[<-] "..target.Group.Prefix..target.Name..target.Group.Postfix..": &e"..message):Send(target)
  else
    server:Log(player.Name..": "..message)
    Message(player.Group.Prefix..player.Name..player.Group.Postfix..": &f"..message):Send(server)
  end
end

server.PlayerLoginEvent:Add(PlayerLogin)
