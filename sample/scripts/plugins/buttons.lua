button = {}
button.__index = button

function button.create(region)
  local object = {
    type = "button",
    level = region.Level,
    region = region,
    blocks_inside = 0,
    activated = "",
    deactivated = "",
    active = false
  }
  setmetatable(object,button)
  object.enter_handler = region.EnterEvent:Add(function(body) object:enter() end)
  object.leave_handler = region.LeaveEvent:Add(function(body) object:leave() end)
  object.block_handler = region.BlockEvent:Add(function(region,args) object:block(args.X,args.Y,args.Z,args.Type) end)
  table.insert(level_buttons,object)
  return object
end

function button:enter()
  if self.active then return end
  self.active = true
  if self.activated then
    assert(loadstring(self.activated))()(self)
  end
end

function button:leave()
  if self.blocks_inside > 0 or self.region.Inside.Count > 0 then return end
  self.active = false
  if self.deactivated then
    assert(loadstring(self.deactivated))()(self)
  end
end

function button:block(x,y,z,block)
  local before = self.level:GetBlock(x,y,z)
  if block ~= 0 and before == 0 then
    self.blocks_inside = self.blocks_inside + 1
    if not self.active then
      self.active = true
      assert(loadstring(self.activated))()(self)
    end
  elseif block == 0 and before ~= 0 then
    self.blocks_inside = self.blocks_inside - 1
    if self.active and self.blocks_inside == 0 and
       self.region.Inside.Count == 0 then
      self.active = false
      assert(loadstring(self.deactivated))()(self)
    end
  end
end

level_buttons = {}

function button_load(level)
  local objects = level.Custom["objects"]
  if objects then
    local buttons = objects["buttons"]
    if buttons then
      buttons:ListForeach(function(node)
        local region = FromCompound(node["region"])
        local button = button.create(Region(server.Level,region.x1,region.y1,region.z1,region.x2,region.y2,region.z2))
        button.active = (node["active"].Value ~= 0)
        button.activated = node["activated"].Value
        button.deactivated = node["deactivated"].Value
        button.blocks_inside = node["blocks"].Value
      end)
    end
  end
  level.Saving:Add(button_save)
end

function button_save(level)
  local objects = level.Custom["objects"]
  if not objects then
    objects = Node()
    level.Custom["objects"] = objects
  end
  local buttons = Node()
  objects["buttons"] = buttons
  for _,button in ipairs(level_buttons) do
    local node = Node()
    buttons:Add(node)
    node["region"] = ToCompound({ x1=button.region.X1,y1=button.region.Y1,z1=button.region.Z1,
                                  x2=button.region.X2,y2=button.region.Y2,z2=button.region.Z2 })
    node["active"] = Node(button.active and 1 or 0)
    node["activated"] = Node(button.activated)
    node["deactivated"] = Node(button.deactivated)
    node["blocks"] = Node(button.blocks_inside)
  end
end

server.InitializedEvent:Add(function() button_load(server.Level) end)
