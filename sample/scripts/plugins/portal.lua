gm_portal = {}
gm_portal.__index = gm_portal

function gm_portal.start(level)
  if not Gravity then dofile "scripts/plugins/gravity.lua" end
  Gravity.blocks[14] = true
  Gravity.blocks[15] = true
  local object = {
    running = true,
    level = level,
    players = {}
  }
  setmetatable(object,gm_portal)
  Gravity.Fall = function(args) object:fall(args) end
  object.handler = server.PlayerLoginEvent:Add(function(player)
    object.players[player] = object:join(player)
  end)
  for i = 0,server.Players.Count-1 do
    object:join(server.Players[i])
  end
  return object
end

function gm_portal:destroy()
  self.running = false
  self.level = nil
  self.players = {}
  server.PlayerLoginEvent:Remove(self.handler)
  for k,v in pairs(self.players) do
    k.MoveEvent:Remove(v.move_handle)
    k.BlockEvent:Remove(v.block_handle)
    k.DisconnectedEvent:Remove(v.leave_handle)
    self:leave(v)
  end
end

function gm_portal:join(player)
  local p = { type = "player", orange = nil, blue = nil, mode = "portal", inlava = false, infizzle = false }
  p.move_handler = player.MoveEvent:Add(function() self:move(player) end)
  p.block_handler = player.BlockEvent:Add(function(player,args,holding)
    local p = self.players[player]
    if p.mode == "build" then return
    elseif p.mode == "portal" then
      self:block(player,args.X,args.Y,args.Z,args.Type,holding)
    end
    args.Abort = true
  end)
  p.leave_handler = player.DisconnectedEvent:Add(function()
    self:leave(p)
  end)
  self.players[player] = p
  return p
end

function gm_portal:leave(p)
  if p.orange then
    for pos,block in pairs(p.orange.blocks) do
      self.level:SetBlock(nil,pos[1],pos[2],pos[3],block)
    end
    p.orange.region:Destroy()
    p.orange = nil
  end
  if p.blue then
    for pos,block in pairs(p.blue.blocks) do
      self.level:SetBlock(nil,pos[1],pos[2],pos[3],block)
    end
    p.blue.region:Destroy()
    p.blue = nil
  end
  p.ignore = nil
end

function gm_portal:block(player,x,y,z,block,holding)
  local p = self.players[player]
  if block ~= 0 then return end
  local b = self.level:GetBlock(x,y,z)
  if Gravity.blocks[b] then
    self:push(player,x,y,z,b)
    return
  end
  if p.infizzle or (holding ~= 22 and holding ~= 28) or
     not self:free(x,y,z) then return end
  local rot = -1
  local size = 2
  local relY = 0
  if self:air(x-1,y,z) then
    rot = 64
  end
  if self:air(x+1,y,z) then
    if rot >= 0 then return else rot = 192 end
  end
  if self:air(x,y-1,z) then
    if rot >= 0 then return else
      rot = math.floor(player.Position.RotX/64+0.5)*64
      size = 1
      relY = -20
    end
  end
  if self:air(x,y+1,z) then
    if rot >= 0 then return else
      rot = math.floor(player.Position.RotX/64+0.5)*64
      size = 1
      relY = 20
    end
  end
  if self:air(x,y,z-1) then
    if rot >= 0 then return else rot = 0 end
  end
  if self:air(x,y,z+1) then
    if rot >= 0 then return else rot = 128 end
  end
  if rot < 0 then return end
  if size == 2 and (not self:free(x,y+1,z) or self:air(x,y+2,z)) then return end
  if holding == 22 then
    self:portal(p,x,y,z,size,relY,"orange","blue",rot,22,11,9)
  elseif holding == 28 then
    self:portal(p,x,y,z,size,relY,"blue","orange",rot,28,9,11)
  end
end

function gm_portal:portal(p,x,y,z,size,relY,from,to,rot,block,from_block,to_block)
  if p[from] then
    for pos,block in pairs(p[from].blocks) do
      self.level:SetBlock(nil,pos[1],pos[2],pos[3],block)
    end
    p[from].region.X1 = x
    p[from].region.Y1 = y
    p[from].region.Z1 = z
    p[from].region.X2 = x+1
    p[from].region.Y2 = y+size
    p[from].region.Z2 = z+1
  else
    local region = Region(self.level,x,y,z,x+1,y+size,z+1)
    region.Tag = p
    p[from] = { region = region, x = 0, z = 0 }
  end
  p[from].rot = rot
  p[from].y = relY
  p[from].blocks = {}
  for x = p[from].region.X1,p[from].region.X2-1 do
    for y = p[from].region.Y1,p[from].region.Y2-1 do
      for z = p[from].region.Z1,p[from].region.Z2-1 do
        p[from].blocks[{x,y,z}] = self.level:GetBlock(x,y,z)
      end
    end
  end
  if p[to] then
    self.level:Cuboid(nil,p[from].region,from_block)
    if not p.ignore then
      self.level:Cuboid(nil,p[to].region,to_block)
      p[from].region.EnterEvent:Add(function(body) self:enter(body,p,p[from],p[to]) end)
      p[to].region.EnterEvent:Add(function(body) self:enter(body,p,p[to],p[from]) end)
      p.ignore = {}
    end
  else self.level:Cuboid(nil,p[from].region,block) end
end

function gm_portal:enter(body,p,from,to)
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
  if not p.ignore[body] then
    body:Teleport(x,y,z,rot,body.Position.RotY)
    p.ignore[body] = true
  else
    p.ignore[body] = nil
  end
end

function gm_portal:move(player)
  local p = self.players[player]
  if p.mode ~= "build" then
    local x = math.floor(player.Position.X/32)
    local y = math.floor(player.Position.Y/32)-1
    local z = math.floor(player.Position.Z/32)
    if x<0 or y<0 or z<0 or x>=self.level.Width or
       y>=self.level.Depth or z>=self.level.Height then return end
    local block = self.level:GetBlock(x,y,z)
    if block == 10 then
      if not p.inlava then
        player:Teleport(self.level.Spawn)
        self:leave(p)
        p.inlava = true
      end
    else p.inlava = false end
    if block == 37 or block == 38 then
      if not p.infizzle then
        self:leave(p)
        p.infizzle = true
      end
    else p.infizzle = false end
  end
end

function gm_portal:push(player,x,y,z,block)
  local rot = math.floor(player.Position.RotX/64+0.5)*64 % 256
  if math.sqrt((x+0.5-player.Position.X/32)^2 +
               (y+1.5-player.Position.Y/32)^2 +
               (z+0.5-player.Position.Z/32)^2) > 2 then return end
  local pos = nil
  if rot == 0 then pos = { x,y,z-1 }
  elseif rot == 64 then pos = { x+1,y,z }
  elseif rot == 128 then pos = { x,y,z+1 }
  else pos = { x-1,y,z } end
  local b = self.level:GetBlock(pos[1],pos[2],pos[3])
  if b == 9 or b == 11 then
    self:tele_block(x,y,z,pos[1],pos[2],pos[3])
  elseif b == 44 then
    self.level:SetBlock(nil,pos[1],pos[2]+1,pos[3],block)
    self.level:SetBlockData(x,y,z,0)
    self.level:SetBlock(nil,x,y,z,0)
  elseif b == 0 then
    self.level:SetBlock(nil,pos[1],pos[2],pos[3],block)
    self.level:SetBlockData(x,y,z,0)
    self.level:SetBlock(nil,x,y,z,0)
  end
end

function gm_portal:fall(args)
  local _,x,y,z,counter = unpack(args)
  local above = self.level:GetBlock(x,y,z)
  if not Gravity.blocks[above] then return end
  if self.level:GetBlockData(x,y,z) ~= counter then return end
  local below = self.level:GetBlock(x,y-1,z)
  if below == 0 then
    self.level:SetBlock(nil,x,y-1,z,above)
    self.level:SetBlockData(x,y,z,0)
    self.level:SetBlock(nil,x,y,z,0)
  elseif below == 9 or below == 11 then
    self:tele_block(x,y,z,x,y-1,z)
  elseif below == 10 then
    self.level:SetBlock(nil,x,y,z,0)
  end
end

function gm_portal:tele_block(xfrom,yfrom,zfrom,xto,yto,zto)
  local block = self.level:GetBlock(xfrom,yfrom,zfrom)
  local regions = self.level:RegionsAt(xto,yto,zto)
  for i = 0,regions.Count-1 do
    local region = regions[i]
    if region.Tag and region.Tag.type and region.Tag.type == "player" then
      local p = region.Tag
      local target = nil
      if region == p.orange.region then
        target = p.blue
      else target = p.orange end
      local pos = nil
      if target.y < 0 then pos = { target.region.X1,target.region.Y1-1,target.region.Z1 }
      elseif target.y > 0 then pos = { target.region.X1,target.region.Y1+1,target.region.Z1 }
      else
        if target.rot == 0 then pos = { target.region.X1,target.region.Y1,target.region.Z1-1 }
        elseif target.rot == 64 then pos = { target.region.X1-1,target.region.Y1,target.region.Z1 }
        elseif target.rot == 128 then pos = { target.region.X1,target.region.Y1,target.region.Z1+1 }
        else pos = { target.region.X1+1,target.region.Y1,target.region.Z1 } end
      end
      if self.level:GetBlock(pos[1],pos[2],pos[3]) == 0 then
        self.level:SetBlock(nil,pos[1],pos[2],pos[3],block)
        self.level:SetBlockData(xfrom,yfrom,zfrom,0)
        self.level:SetBlock(nil,xfrom,yfrom,zfrom,0)
      else
        self.level:SetBlockData(xfrom,yfrom,zfrom,Gravity.counter+1)
        server.Queue:Add(400,Gravity.Fall,{nil,xfrom,yfrom,zfrom,Gravity.counter+1})
        Gravity.counter = (Gravity.counter + 1) % 255
      end
    end
  end
end

function gm_portal:free(x,y,z)
  local block = self.level:GetBlock(x,y,z)
  if block == 5 then return true
  elseif block == 17 then return true
  elseif block == 47 then return true
  else return false end
end

function gm_portal:air(x,y,z)
  local block = self.level:GetBlock(x,y,z)
  if block == 0 then return true
  elseif block == 14 then return true
  elseif block == 15 then return true
  else return false end
end

Command("mode","[<player>] none|portal|build","Changes the mode of a player.",
  function(command,player,message)
    if message == "" then
      Message("&eSyntax: "..command.syntax):Send(player)
      return
    end
    local args = message:split(" ",2)
    args[#args] = args[#args]:lower()
    if args[#args] ~= "none" and args[#args] ~= "portal"
       and args[#args] ~= "build" then
      Message("&eSyntax: "..command.syntax):Send(player)
      return
    end
    if portal_inst == nil then
      Message("&ePortal gamemode isn't running."):Send(player)
      return
    end
    local target = player
    if #args == 2 then
      target = FindPlayer(args[1])
      if target == nil then
        Message("&eThere is no player '"..args[1].."'."):Send(player)
        return
      end
    end
    portal_inst.players[target].mode = args[#args]
    if player ~= target then
      Message("&e"..target.Name.." is now in mode '"..args[#args].."'."):Send(player)
    end
    Message("&eYou are now in mode '"..args[#args].."'."):Send(target)
  end
)

portal_inst = gm_portal.start(server.Level)
dofile "scripts/plugins/buttons.lua"
dofile "scripts/plugins/platforms.lua"
