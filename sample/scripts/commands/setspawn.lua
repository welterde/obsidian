Command("setspawn","","Sets the spawn location to your current position.",
  function(command,player,message)
    if message ~= "" then
      Message("&eSyntax: "..command.syntax):Send(player)
	  return
    end
    player.Level.Spawn:Set(player.Position)
    Message("&eSpawn position changed."):Send(player)
  end
)
