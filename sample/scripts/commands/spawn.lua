Command("spawn","","Respawns you at the default spawn position.",
  function(command,player,message)
    if message ~= "" then
      Message("&eSyntax: "..command.syntax):Send(player)
      return
    end
    player:Spawn(player.Level.Spawn)
  end
)
