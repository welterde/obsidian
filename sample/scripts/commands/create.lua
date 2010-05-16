Command("create","<object>","Creates various stuff.",
  function(command,player,message)
    Message("&eSyntax: "..command.syntax):Send(player)
  end
)
dofile "scripts/commands/create_portal.lua"