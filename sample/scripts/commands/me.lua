Command("me","","Makes you do something. LOL!",
  function(command,player,message)
    if message == "" then
      Message("&eSyntax: "..command.syntax):Send(player)
	  return
    end
    Message("&e*"..player.Group.Prefix..player.Name..player.Group.Postfix.."&e "..message):Send(player.Level)
  end
)
