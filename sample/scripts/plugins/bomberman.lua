bomberman = {}
bomberman.__index = bomberman

function bomberman.create(level,x1,y1,z1,x2,y2,z2)
  local object = {
    running = true,
    level = level,
    x1 = x1, y1 = y1, z1 = z1,
    x2 = x2, y2 = y2, z2 = z2,
    rand = math.random(),
    powerups = 0,
    counter = 0,
    players = {},
    blocks = {},
    region = Region(level,x1,y1,z1,x2,y2,z2)
  }
  setmetatable(object,bomberman)
  object.region.BlockEvent:Add(function(region,args)
    args.Abort = not object:block(args.Origin,args.X,args.Y,args.Z,args.Type)
  end)
  object.region.Destroyed:Add(function() object:destroyed() end)
  local region = Region(level,x1,y1,z2,x2,y2,z2+1)
  region.EnterEvent:Add(function(player) object:dead(player) end)
  object:powerup(object.rand)
  return object
end

function bomberman:destroyed()
  if not self.running then return end
  for k,v in pairs(self.players) do
    k.DisconnectedEvent:Remove(v.handle)
  end
  self.running = false
end

function bomberman:block(player,x,y,z,block)
  if not self.running then return true end
  if not player then return true end
  if y ~= self.y1+1 or
     not self.region:IsInside(player) or
     self.level:GetBlock(x,y,z) ~= 0 or
     self.level:GetBlock(x,y-1,z) == 11 then
    return false
  end
  local p = self:get_player(player)
  if block == 46 then
    if p.bombs >= p.max_bombs then return false end
    p.bombs = p.bombs + 1
    local index = x-self.x1+(z-self.z1)*(self.x2-self.x1)
    self.blocks[index] = p
    self:bomb(player,x,y,z,p.range)
    return true
  elseif block == 5 then
    if p.walls == 0 then return false end
    p.walls = p.walls - 1
    self:wall(player,x,y,z)
    return true
  end
  return false
end

function bomberman:get_player(player)
  local p = self.players[player]
  if not p then
    p = { bombs = 0, walls = 6, max_bombs = 4, range = 4 }
    p.handle = player.DisconnectedEvent:Add(function() self:player_left(player) end)
    self.players[player] = p
  end
  return p
end

function bomberman:bomb(player,x,y,z,range)
  if not self.running then return end
  local counter = self.counter
  self.level:SetBlockData(x,y,z,counter)
  server.Queue:Add(1700,function() self:explode(player,x,y,z,counter) end,nil)
  self.counter = (self.counter + 1) % 256
end

function bomberman:wall(player,x,y,z)
  if not self.running then return end
  self.level:SetBlock(nil,x,y+1,z,5)
  self.level:SetBlock(nil,x,y+2,z,5)
end

function bomberman:powerup(rand)
  if not self.running then return end
  if self.rand ~= rand then return end
  server.Queue:Add(math.random(8000,14000),function() self:powerup(rand) end,nil)
  if math.random()*(self.powerups+1)/server.Players.Count < 0.5 then
    while true do
      local x = math.random(self.x1,self.x2-1)
      local y = self.y1+1
      local z = math.random(self.z1,self.z2-1)
      if self.level:GetBlock(x,y,z) == 0 and
         self.level:GetBlock(x,y-1,z) == 34 then
        self:place_powerup(x,y,z)
        return
      end
    end
  end
end

function bomberman:place_powerup(x,y,z)
  if not self.running then return end
  self.level:SetBlock(nil,x,y-1,z,41)
  self.level:SetBlock(nil,x,y,z,9)
  self.level:SetBlock(nil,x,y+1,z,9)
  self.level:SetBlock(nil,x,y+2,z,42)
  local region = Region(self.level,x,y,z,x+1,y+3,z+1)
  local index = x-self.x1 + (z-self.z1)*(self.x2-self.x1)
  region.EnterEvent:Add(function(player) self:pickup_powerup(player,x,y,z,index) end)
  self.blocks[index] = region
  self.powerups = self.powerups + 1
end

function bomberman:pickup_powerup(player,x,y,z,index)
  if not self.running then return end
  if player.Position.Y <= self.y1*32+80 then return end
  if not Is(player,"obsidian.World.Player") then return end
  local p = self:get_player(player)
  if p.max_bombs == 8 and p.range == 12 and p.walls == 28 then
    Message("&eYou reached the maximum amount of powerups."):Send(player)
    return
  end
  self.level:SetBlock(nil,x,y-1,z,34)
  self.level:SetBlock(nil,x,y,z,0)
  self.level:SetBlock(nil,x,y+1,z,0)
  self.level:SetBlock(nil,x,y+2,z,20)
  self.blocks[index]:Destroy()
  self.blocks[index] = nil
  self.powerups = self.powerups - 1
  local min,max = 1,3
  if p.max_bombs >= 8 then
    min = min + 1
  end
  if p.walls >= 28 then
    max = max - 1
  end
  local powerup = math.random(min,max)
  if powerup == 1 then
    p.max_bombs = p.max_bombs + 2
    Message("&eYou collected additional bombs. You now have "..p.max_bombs.."."):Send(player)
  elseif powerup == 2 and p.range < 12 then
    p.range = p.range + 2
    Message("&eYou collected additional firepower. Your range is "..p.range.."."):Send(player)
  else
    p.walls = math.min(28,p.walls+6)
    Message("&eYou collected additional walls. You now have "..p.walls.."."):Send(player)
  end
end

function bomberman:explode(player,x,y,z,counter)
  if not self.running then return end
  if self.level:GetBlock(x,y,z) == 46 and
     self.level:GetBlockData(x,y,z) == counter then
    self:destroy(x,y,z)
  end
end

function bomberman:destroy(x,y,z,counter)
  if not self.running then return false end
  if not self.region:IsInside(x,y,z) then return false end
  local block = self.level:GetBlock(x,y,z)
  if block == 7 then return false end
  self.level:SetBlock(nil,x,y,z,0)
  if block == 5 then
    self.level:SetBlock(nil,x,y+1,z,0)
    self.level:SetBlock(nil,x,y+2,z,20)
    return false
  end
  if block == 9 then
    local index = x-self.x1 + (z-self.z1)*(self.x2-self.x1)
    self.blocks[index]:Destroy()
    self.blocks[index] = nil
    self.level:SetBlock(nil,x,y-1,z,34)
    self.level:SetBlock(nil,x,y+1,z,0)
    self.level:SetBlock(nil,x,y+2,z,20)
    self.powerups = self.powerups - 1
    return false
  end
  if block == 46 then
    counter = self.level:GetBlockData(x,y,z)
    local index = x-self.x1 + (z-self.z1)*(self.x2-self.x1)
    local p = self.blocks[index]
    p.bombs = p.bombs - 1
    for i = 1,p.range do if not self:destroy(x+i,y,z,counter) then break end end
    for i = 1,p.range do if not self:destroy(x-i,y,z,counter) then break end end
    for i = 1,p.range do if not self:destroy(x,y,z+i,counter) then break end end
    for i = 1,p.range do if not self:destroy(x,y,z-i,counter) then break end end
    server.Queue:Add(1200,function() self:clear(self.level,x,y,z,counter) end,nil)
  else self.level:SetBlockData(x,y,z,counter) end
  self.level:SetBlock(nil,x,y-1,z,11)
  return true
end

function bomberman:clear(level,x,y,z,counter)
  if not self.running then return end
  self:clear_one(x,y,z,counter)
  for i = 1,12 do self:clear_one(x+i,y,z,counter) end
  for i = 1,12 do self:clear_one(x-i,y,z,counter) end
  for i = 1,12 do self:clear_one(x,y,z+i,counter) end
  for i = 1,12 do self:clear_one(x,y,z-i,counter) end
  local index = x-self.x1 + (z-self.z1)*(self.x2-self.x1)
  self.blocks[index] = nil
end

function bomberman:clear_one(x,y,z,counter)
  if self.level:GetBlock(x,y,z) == 0 and
     self.level:GetBlockData(x,y,z) == counter then
    self.level:SetBlock(nil,x,y-1,z,34)
  end
end

function bomberman:teleport(player)
  if not Is(player,"obsidian.World.Player") then return end
  while true do
    local x = math.random(self.x1,self.x2-1)
    local y = self.y1+1
    local z = math.random(self.z1,self.z2-1)
    if self.level:GetBlock(x,y,z) == 0 then
      player:Teleport(x*32+16,y*32+48,z*32+16,
        player.Position.RotX+math.random(0,3)*90,
        player.Position.RotY)
      return
    end
  end
end

function bomberman:dead(player)
  if not self.running then return end
  if not Is(player,"obsidian.World.Player") then return end
  local p = self:get_player(player)
  if p.max_bombs > 4 or p.range > 4 or p.walls > 4 then
    Message("&c*"..player.Group.Prefix..player.Name..player.Group.Postfix.."&c died and lost some powerups."):Send(self.level)
    p.max_bombs = math.max(4,p.max_bombs-2)
    p.range = math.max(4,p.range-2)
    p.walls = math.max(4,p.walls-6)
    Message("&eYou have "..p.max_bombs.." bombs with a range of "..p.range..
            " and "..p.walls.." walls left."):Send(player)
    local x = math.floor(player.Position.X/32)
    local y = self.y1+1
    local z = math.floor(player.Position.Z/32)
    if self.level:GetBlock(x,y,z) == 0 then
      self:place_powerup(x,y,z)
    end
  else
    Message("&c*"..player.Group.Prefix..player.Name..player.Group.Postfix.."&c died."):Send(self.level)
  end
  player:Teleport(self.level.Spawn)
end

function bomberman:player_left(player)
  self.players[player] = nil
end

function bomberman.init(level)
  server.Help = "&eThis server is running a bomberman gamemode. "..
                "Use the teleporters to join the fun. Collect powerups "..
                "to increase the limit and range of your bombs "..
                "as well as walls.\n"..
                "&eTo show a list of commands, type '/help commands'."
  local object = bomberman.create(level,8,0,8,level.Width-8,4,level.Height-8)
  level.BlockEvent:Add(function(level,args)
    if not object.running then return end
    if not object.region:IsInside(args.X,args.Y,args.Z) then
      args.Abort = true
    end
  end)
  Region(level,62,5,8,63,7,9).EnterEvent:Add(function(player) object:teleport(player) end)
  Region(level,55,5,1,56,7,2).EnterEvent:Add(function(player) object:teleport(player) end)
  Region(level,58,1,36,59,3,37).EnterEvent:Add(function(player) object:teleport(player) end)
  Region(level,58,1,27,59,3,28).EnterEvent:Add(function(player) object:teleport(player) end)
  Region(level,27,1,5,28,3,6).EnterEvent:Add(function(player) object:teleport(player) end)
  Region(level,36,1,5,37,3,6).EnterEvent:Add(function(player) object:teleport(player) end)
  Region(level,8,0,8,level.Width-8,1,level.Height-8).EnterEvent:Add(function(player) object:dead(player) end)
end

bomberman.init(server.Level)
