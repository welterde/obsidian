Command("tp","<player>","Teleports yourself to a player.",
  function(command,player,message)
    if message == "" then
      Message("&eSyntax: "..command.syntax):Send(player)
	  return
    end
	local target = FindPlayer(message)
	if target == nil then
	  Message("&eThere is no player '"..message.."'."):Send(player)
	  return
	end
	if target == player then
	  Message("&eYou can't teleport to yourself."):Send(player)
	  return
	end
    player:Teleport(target.Position)
  end
)