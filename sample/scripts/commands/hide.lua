Command("hide","[<player>]","Hides a player.",
  function(command,player,message)
    local target = player
    if message ~= "" then
	  target = FindPlayer(message)
	  if target == nil then
	    Message("&eThere is no player '"..message.."'."):Send(player)
	    return
	  end
    end
    if not target.Visible then
      local name = nil
      if target ~= player then
        name = player.Name.." is"
      else name = "You are" end
      Message("&e"..name.." is already hidden."):Send(player)
      return
    end
    target.Visible = false
  end
)
