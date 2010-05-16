levelPortals = {}

function LevelLoad(level)
  local objects = level.Custom["objects"]
  if objects then
    local portals = objects["portals"]
    if portals then
      portals:ListForeach(function(node)
        local first = FromCompound(node["first"])
        local second = FromCompound(node["second"])
        local portal = Portal(level,first.x,first.y,first.z,second.x,second.y,second.z)
        portal.Orientation = node["orientation"]
        table.insert(levelPortals,portal)
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
end

function ToCompound(t)
  local node = Node()
  for k,v in pairs(t) do
    node[k] = Node(v)
  end
  return node
end
function FromCompound(node)
  t = {}
  node:DictForeach(function(name,n) t[name] = n.Value end)
  return t
end

server.InitializedEvent:Add(function() LevelLoad(server.Level) end)