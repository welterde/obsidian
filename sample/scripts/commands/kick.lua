Command("kick","<player> [<message>]","Kicks a player from the server.",
  function(command,player,message)
    if message == "" then
      Message("&eSyntax: "..command.syntax):Send(player)
	  return
    end
	local args = message:split(" ",2)
	local target = FindPlayer(args[1])
	if target == nil then
	  Message("&eThere is no player '"..message.."'."):Send(player)
	  return
	end
	if target == player then
	  Message("&eYou can't kick yourself."):Send(player)
	  return
	end
    if target.Group == admin and player.Group ~= admin then
      Message("&eYou can't kick an admin."):Send(player)
	  return
    end
	target:Kick(args[2] or "Kicked by "..player.Name)
  end
)