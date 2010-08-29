levelPortals = {}
levelDummies = {}

function LevelLoad(level)
  local objects = level.Custom["objects"]
  if objects then
    local portals = objects["portals"]
    if portals then
      portals:ListForeach(function(node)
        local first = FromCompound(node["first"])
        local second = FromCompound(node["second"])
        local portal = Portal(level,first.x,first.y,first.z,second.x,second.y,second.z)
        portal.Orientation = node["orientation"].Value
        table.insert(levelPortals,portal)
      end)
    end
    local dummies = objects["dummies"]
    if dummies then
      dummies:ListForeach(function(node)
        local name = node["name"].Value
        local pos = FromCompound(node["pos"])
        local dummy = Body(name,level)
        dummy.Position:Set(pos.x,pos.y,pos.z,pos.rotx,pos.roty)
        dummy.Visible = true
        table.insert(levelDummies,dummy)
      end)
    end
  end
end

function LevelSave(level)
  local objects = Node()
  level.Custom["objects"] = objects
  local portals = Node()
  objects["portals"] = portals
  for i,portal in ipairs(levelPortals) do
    local node = Node()
    portals:Add(node)
    node["first"] = ToCompound({ x=portal.First.X1,y=portal.First.Y1,z=portal.First.Z1 })
    node["second"] = ToCompound({ x=portal.Second.X1,y=portal.Second.Y1,z=portal.Second.Z1 })
    node["orientation"] = Node(portal.Orientation)
  end
  local dummies = Node()
  objects["dummies"] = dummies
  for i,dummy in ipairs(levelDummies) do
    local node = Node()
    dummies:Add(node)
    node["name"] = Node(dummy.Name)
    node["pos"] = ToCompound({ x=dummy.Position.X,y=dummy.Position.Y,z=dummy.Position.Z,
                               rotx=dummy.Position.RotX,roty=dummy.Position.RotY })
  end
end

server.InitializedEvent:Add(function() LevelLoad(server.Level) end)
