Command("motd","","Shows the message of the day, in case you missed it.",
  function(command,player,message)
    if message ~= "" then
      Message("&eSyntax: "..command.syntax):Send(player)
	  return
    end
	Message("&e"..server.motd):Send(player)
  end
)