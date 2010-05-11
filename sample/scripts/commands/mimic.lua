Command("mimic","[player]","Create a mimic which followes a player.",
  function(command,player,message)
    local target = player
    if message ~= "" then
	  target = FindPlayer(message)
	  if target == nil then
	    Message("&eThere is no player '"..message.."'."):Send(player)
	    return
	  end
    end
	Mimic(target)
  end
)