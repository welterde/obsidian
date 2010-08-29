Command("dummy","[destroy] <name>","Creates a dummy.",
  function(command,player,message)
    if message == "" then
      Message("&eSyntax: "..command.syntax):Send(player)
      return
    end
    local body = Body(message,player.Level)
    body.Position:Set(player.Position)
    body.Visible = true
    table.insert(level_dummies,body)
  end
)
dofile("scripts/commands/dummy_destroy.lua")

level_dummies = {}

function dummy_load(level)
  local objects = level.Custom["objects"]
  if objects then
    local dummies = objects["dummies"]
    if dummies then
      dummies:ListForeach(function(node)
        local name = node["name"].Value
        local pos = FromCompound(node["pos"])
        local dummy = Body(name,level)
        dummy.Position:Set(pos.x,pos.y,pos.z,pos.rotx,pos.roty)
        dummy.Visible = true
        table.insert(level_dummies,dummy)
      end)
    end
  end
  level.Saving:Add(dummy_save)
end

function dummy_save(level)
  local objects = level.Custom["objects"]
  if not objects then
    objects = Node()
    level.Custom["objects"] = objects
  end
  local dummies = Node()
  objects["dummies"] = dummies
  for i,dummy in ipairs(level_dummies) do
    local node = Node()
    dummies:Add(node)
    node["name"] = Node(dummy.Name)
    node["pos"] = ToCompound({ x=dummy.Position.X,y=dummy.Position.Y,z=dummy.Position.Z,
                               rotx=dummy.Position.RotX,roty=dummy.Position.RotY })
  end
end

server.InitializedEvent:Add(function() dummy_load(server.Level) end)
