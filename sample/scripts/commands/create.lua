Command("create","<entity>","Creates various stuff.",
  function(command,player,message)
    Message("&eSyntax: "..command.syntax):Send(player)
  end
)
dofile "scripts/commands/create_portal.lua"
dofile "scripts/commands/create_portalbutton.lua"