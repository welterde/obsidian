Command("summon","<player>","Summons another player to your position.",
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
	  Message("&eYou can't summon yourself."):Send(player)
	return
	end
    target:Teleport(player.Position)
  end
)