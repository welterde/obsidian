Command("guest","<name>","Changes the group of a player to guest.",
  function(command,player,message)
    if message == "" then
      Message("&eSyntax: "..command.syntax):Send(player)
	  return
    end
	local target = FindPlayer(message)
	if target == nil then
	  target = FindAccount(message)
	  if target == nil then
	    Message("&eUnknown player or account '"..message.."'."):Send(player)
	    return
	  end
	else
	  target = target.Account
	end
    local group = guest
	if target.Group == group then
	  Message("&e"..target.Name.." already is in this group."):Send(player)
	  return
	end
    if player.Group ~= admin and (target.Group == admin or target.Group == operator) then
      Message("&eYou can't change the group of an "..target.Group.Name.."."):Send(player)
	  return
	end
	Message("&e*"..target.Group.Prefix..target.Name..target.Group.Postfix.."&e is now "..
	        group.Prefix..group.Name..group.Postfix.."&e."):Send(player.Level)
    target.Group = group
	target:Save()
  end
)