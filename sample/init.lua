server.Name = "Custom Minecraft server";
server.Motd = "Welcome to my custom Minecraft server!";
server.Port = 25565;
server.Public = false
server.Slots = 16;
server.MainLevel = "main"

-- Load needed files
dofile "scripts/functions.lua"

dofile "scripts/level.lua"
dofile "scripts/player.lua"
dofile "scripts/messages.lua"
dofile "scripts/commands.lua"

-- Load plugins
dofile "scripts/plugins/stairs.lua"
dofile "scripts/plugins/gravity.lua"
dofile "scripts/plugins/grassgrowth.lua"

-- Activate plugins
server.InitializedEvent:Add(
  function()
    Stairs.Start(server.Level)
    Gravity.Start(server.Level)
    Grassgrowth.Start(server.Level)
  end
)
