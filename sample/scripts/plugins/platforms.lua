platform = {}
platform.__index = platform

function platform.create(level,x,y,z,width,height)
  local object = {
    type = "platform",
    level = level,
    x = x, y = y, z = z,
    width = width, height = height,
    block = 42,
    direction = 0,
    speed = 750,
    waypoints = {}
  }
  setmetatable(object,platform)
  table.insert(level_platforms,object)
  return object
end

function platform:start()
  for x = self.x, self.x+self.width-1 do
    for z = self.z, self.z+self.height-1 do
      if self.level:GetBlock(x,self.y,z) == 0 then
        self.level:SetBlock(nil,x,self.y,z,self.block)
      end
    end
  end
  server.Queue:Add(self.speed,function() self:move() end,nil)
end

function platform:move()
  if self.direction == 0 then
    for z = self.z,self.z+self.height-1 do
      if self.level:GetBlock(self.x,self.y,z) == self.block then
        self.level:SetBlock(nil,self.x,self.y,z,0)
      end
    end
    self.x = self.x + 1
    for z = self.z,self.z+self.height-1 do
      if self.level:GetBlock(self.x+self.width-1,self.y,z) == 0 then
        self.level:SetBlock(nil,self.x+self.width-1,self.y,z,self.block)
      end
    end
  elseif self.direction == 1 then
    for x = self.x,self.x+self.width-1 do
      if self.level:GetBlock(x,self.y,self.z) == self.block then
        self.level:SetBlock(nil,x,self.y,self.z,0)
      end
    end
    self.z = self.z + 1
    for x = self.x,self.x+self.width-1 do
      if self.level:GetBlock(x,self.y,self.z+self.height-1) == 0 then
        self.level:SetBlock(nil,x,self.y,self.z+self.height-1,self.block)
      end
    end
  elseif self.direction == 2 then
    for z = self.z,self.z+self.height-1 do
      if self.level:GetBlock(self.x+self.width-1,self.y,z) == self.block then
        self.level:SetBlock(nil,self.x+self.width-1,self.y,z,0)
      end
    end
    self.x = self.x - 1
    for z = self.z,self.z+self.height-1 do
      if self.level:GetBlock(self.x,self.y,z) == 0 then
        self.level:SetBlock(nil,self.x,self.y,z,self.block)
      end
    end
  elseif self.direction == 3 then
    for x = self.x,self.x+self.width-1 do
      if self.level:GetBlock(x,self.y,self.z+self.height-1) == self.block then
        self.level:SetBlock(nil,x,self.y,self.z+self.height-1,0)
      end
    end
    self.z = self.z - 1
    for x = self.x,self.x+self.width-1 do
      if self.level:GetBlock(x,self.y,self.z) == 0 then
        self.level:SetBlock(nil,x,self.y,self.z,self.block)
      end
    end
  end
  server.Queue:Add(self.speed,function() self:move() end,nil)
  for point,code in pairs(self.waypoints) do
    if point[1] == self.x and point[2] == self.z then
      self:waypoint(code)
    end
  end
end

function platform:waypoint(code)
  assert(loadstring(code))()(self)
end

level_platforms = {}

function platform_load(level)
  local objects = level.Custom["objects"]
  if objects then
    local platforms = objects["platforms"]
    if platforms then
      platforms:ListForeach(function(node)
        local region = FromCompound(node["region"])
        local platform = platform.create(server.Level,region.x,region.y,region.z,region.width,region.height)
        platform.block = node["block"].Value
        platform.direction = node["direction"].Value
        platform.speed = node["speed"].Value
        local waypoints = node["waypoints"]
        if waypoints then
          waypoints:ListForeach(function(node)
            local x = node["x"].Value
            local z = node["z"].Value
            local code = node["code"].Value
            platform.waypoints[{x,z}] = code
          end)
        end
        server.Queue:Add(platform.speed,function() platform:move() end,nil)
      end)
    end
  end
  level.Saving:Add(platform_save)
end

function platform_save(level)
  local objects = level.Custom["objects"]
  if not objects then
    objects = Node()
    level.Custom["objects"] = objects
  end
  local platforms = Node()
  objects["platforms"] = platforms
  for _,platform in ipairs(level_platforms) do
    local node = Node()
    platforms:Add(node)
    node["region"] = ToCompound({ x=platform.x,y=platform.y,z=platform.z,width=platform.width,height=platform.height })
    node["block"] = Node(platform.block)
    node["direction"] = Node(platform.direction)
    node["speed"] = Node(platform.speed)
    local waypoints = Node()
    node["waypoints"] = waypoints
    for point,code in pairs(platform.waypoints) do
      local node = Node()
      waypoints:Add(node)
      node["x"] = Node(point[1])
      node["z"] = Node(point[2])
      node["code"] = Node(code)
    end
  end
end

server.InitializedEvent:Add(function() platform_load(server.Level) end)
