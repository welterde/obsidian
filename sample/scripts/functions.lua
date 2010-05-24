-- Import types
luanet.load_assembly("obsidian")
Message = luanet.import_type("obsidian.Net.Message")
Region = luanet.import_type("obsidian.World.Region")
Portal = luanet.import_type("obsidian.World.Objects.Portal")
Body = luanet.import_type("obsidian.World.Body")
Blocktype = luanet.import_type("obsidian.World.Blocktype")
Node = luanet.import_type("obsidian.Data.Node")
UpdateQueue = luanet.import_type("obsidian.Control.UpdateQueue")
Command = function(name,syntax,help,func)
  server.Commands:Create(name,syntax,help,func)
end

-- Load plugins
dofile "scripts/plugins/stairs.lua"
dofile "scripts/plugins/gravity.lua"
dofile "scripts/plugins/grassgrowth.lua"

server.InitializedEvent:Add(
  function()
    guest = FindGroup("guest")
    trusted = FindGroup("trusted")
    operator = FindGroup("operator")
    admin = FindGroup("admin")
    owner = FindGroup("owner")
    -- Activate plugins
    Stairs.Start(server.Level)
    Gravity.Start(server.Level)
    Grassgrowth.Start(server.Level)
  end
)

-- Functions
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

function table.removevalue(t,value)
  for i,v in ipairs(t) do
    if v == value then
      table.remove(t,i)
      return
    end
  end
end

function table.clone(t,deep)
  local new = {}
  for k,v in pairs(t) do
    if deep and type(v)=="table" then
      v = table.clone(v,true)
    end
    new[k] = v
  end
  return setmetatable(new,getmetatable(t))
end
