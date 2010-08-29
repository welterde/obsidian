portal = {}
portal.__index = portal

function portal.create(first,second)
  local object = {
    type = "portal",
    level = first.Level,
    first = { region = first, rot = 0, x = 0, y = 0, z = 0 },
    second = { region = second, rot = 0, x = 0, y = 0, z = 0 },
    blocks = {},
    ignore = {}
  }
  setmetatable(object,portal)
  first.Tag = object
  second.Tag = object
  for x = first.X1,first.X2-1 do
    for y = first.Y1,first.Y2-1 do
      for z = first.Z1,first.Z2-1 do
        object.blocks[{x,y,z}] = object.level:GetBlock(x,y,z)
      end
    end
  end
  for x = second.X1,second.X2-1 do
    for y = second.Y1,second.Y2-1 do
      for z = second.Z1,second.Z2-1 do
        object.blocks[{x,y,z}] = object.level:GetBlock(x,y,z)
      end
    end
  end
  object.level:Cuboid(nil,first,11)
  object.level:Cuboid(nil,second,9)
  object.first.enter_handler = first.EnterEvent:Add(function(body) object:enter(body,object.first,object.second) end)
  object.second.enter_handler = second.EnterEvent:Add(function(body) object:enter(body,object.second,object.first) end)
  return object
end

function portal:destroy()
  for pos,block in pairs(self.blocks) do
    self.level:SetBlock(nil,pos[1],pos[2],pos[3],block)
  end 
  self.first.region.EnterEvent:Remove(self.first.enter_handler)
  self.second.region.EnterEvent:Remove(self.second.enter_handler)
  self.first.region:Destroy()
  self.second.region:Destroy()
end

function portal:enter(body,from,to)
  local rot = body.Position.RotX
  local x = to.region.X1*32 + to.region.Width*16 + to.x
  local y = to.region.Y1*32 + to.region.Depth*16 + to.y
  local z = to.region.Z1*32 + to.region.Height*16 + to.z
  if from.region.Depth > 1 then
    rot = from.rot - to.rot + rot + 128
  elseif to.region.Depth <= 1 then
    rot = from.rot - to.rot + rot
  else
    if to.rot == 64 or to.rot == 192 then
      rot = to.rot + (body.Position.RotX+32) % 64 + 96
    else rot = to.rot + (body.Position.RotX+32) % 64 - 32 end
  end
  if to.region.Depth > 1 then
    x = x - math.sin(to.rot/128 * math.pi)*14
    z = z - math.cos(to.rot/128 * math.pi)*14
  end
  if not self.ignore[body] then
    body:Teleport(x,y,z,rot,body.Position.RotY)
    self.ignore[body] = true
  else
    self.ignore[body] = nil
  end
end
