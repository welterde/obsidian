luanet.load_assembly("obsidian")
Message = luanet.import_type("obsidian.Net.Message")
Region = luanet.import_type("obsidian.World.Region")
Teleporter = luanet.import_type("obsidian.World.Objects.Teleporter")
Portal = luanet.import_type("obsidian.World.Objects.Portal")
Button = luanet.import_type("obsidian.World.Objects.StateButton")
Blocktype = luanet.import_type("obsidian.World.Blocktype")
Mimic = luanet.import_type("obsidian.World.NPCs.Mimic")
Command = function(name,syntax,help,func)
  server.Commands:Create(name,syntax,help,func)
end

server.InitializedEvent:Add(
  function()
    guest = FindGroup("guest")
    trusted = FindGroup("trusted")
    operator = FindGroup("operator")
    admin = FindGroup("admin")
  end
)

playerNextBlock = {}
playerCode = {}

function FindPlayer(name)
  local i
  name = name:lower()
  for i = 0,server.Players.Count-1 do
    local player = server.Players[i]
	if player.Name:lower() == name then
	  return player
	end
  end
  return nil
end

function FindGroup(name)
  return server.Groups[name]
end

function FindAccount(name)
  return server.Accounts[name]
end

function string:split(sep,limit) 
  local result,pos = {},0
  if limit == nil or limit>1 then
    for st,sp in function() return self:find(sep,pos,true) end do
      table.insert(result,self:sub(pos,st-1))
      pos = sp+1
      if limit and #result==limit-1 then break end
    end
  end
  table.insert(result,string.sub(self,pos))
  return result
end