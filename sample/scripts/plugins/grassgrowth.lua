Grassgrowth = {
  running = false,
  Start = function(level)
    if Grassgrowth.running then error("Already running.") end
    Grassgrowth.level = level
    Grassgrowth.handler = level.BlockEvent:Add(Grassgrowth.Block)
    Grassgrowth.running = true
  end,
  Stop = function()
    if not Grassgrowth.running then error("Not running.") end
    Grassgrowth.level.BlockEvent:Remove(Grassgrowth.handler)
    Grassgrowth.running = false
  end,
  Block = function(level,args)
    local x,y,z = args.X,args.Y,args.Z
    local above = Blocktype.air
    local below = nil
    if args.Type == 2 or args.Type == 3 then
      level:SetBlockData(x,y,z,0)
      if y ~= 0 and level:GetBlock(x,y-1,z) == 2 and level:GetBlockData(x,y-1,z) ~= 1 then
        level:SetBlockData(x,y-1,z,1)
        server.Queue:Add(math.random(9,12)*1000,Grassgrowth.Die,{x,y-1,z})
      end
      if y ~= level.Depth-1 then 
        above = Blocktype.FindById(level:GetBlock(x,y+1,z))
      end
      below = args.Type
    else
      if y == 0 then return end
      y = y - 1
      above = Blocktype.FindById(args.Type)
      below = level:GetBlock(x,y,z)
    end
    if level:GetBlockData(x,y,z) ~= 1 then
      if below == 2 then
        if above.Opaque then
          server.Queue:Add(math.random(9,12)*1000,Grassgrowth.Die,{x,y,z})
        else
          for xx = math.max(-2,-x),math.min(2,level.Width-1-x) do
            for yy = math.max(-2,-y),math.min(2,level.Depth-1-y) do
              for zz = math.max(-2,-z),math.min(2,level.Height-1-z) do
                if math.abs(xx)+math.abs(yy)+math.abs(zz)<4 and level:GetBlock(x+xx,y+yy,z+zz) == 3 and
                   (y+yy == level.Depth-1 or not Blocktype.FindById(level:GetBlock(x+xx,y+yy+1,z+zz)).Opaque) then
                  level:SetBlockData(x+xx,y+yy,z+zz,1)
                  server.Queue:Add(math.random(28,38)*1000,Grassgrowth.Grow,{x+xx,y+yy,z+zz})
                end
              end
            end
          end
        end
      elseif not above.Opaque and below == 3 then
        for xx = math.max(-2,-x),math.min(2,level.Width-1-x) do
          for yy = math.max(-2,-y),math.min(2,level.Depth-1-y) do
            for zz = math.max(-2,-z),math.min(2,level.Height-1-z) do
              if math.abs(xx)+math.abs(yy)+math.abs(zz)<4 and level:GetBlock(x+xx,y+yy,z+zz) == 2 then
                level:SetBlockData(x,y,z,1)
                server.Queue:Add(math.random(28,38)*1000,Grassgrowth.Grow,{x,y,z})
                return;
              end
            end
          end
        end
      end
    end
  end,
  Die = function(args)
    if not Grassgrowth.running then return end
    local x,y,z = unpack(args)
    local level = Grassgrowth.level
    level:SetBlockData(x,y,z,0)
    local below = level:GetBlock(x,y,z);
    if below ~= 2 then return end
    local above = Blocktype.air
    if y ~= level.Depth-1 then 
      above = Blocktype.FindById(level:GetBlock(x,y+1,z))
    end
    if not above.Opaque then return end
    level:SetBlock(nil,x,y,z,3)
  end,
  Grow = function(args)
    if not Grassgrowth.running then return end
    local x,y,z = unpack(args)
    local level = Grassgrowth.level
    level:SetBlockData(x,y,z,0)
    local below = level:GetBlock(x,y,z);
    if below ~= 3 then return end
    local above = Blocktype.air
    if y ~= level.Depth-1 then 
      above = Blocktype.FindById(level:GetBlock(x,y+1,z))
    end
    if above.Opaque then return end
    for xx = math.max(-2,-x),math.min(2,level.Width-1-x) do
      for yy = math.max(-2,-y),math.min(2,level.Depth-1-y) do
        for zz = math.max(-2,-z),math.min(2,level.Height-1-z) do
          if math.abs(xx)+math.abs(yy)+math.abs(zz)<4 and level:GetBlock(x+xx,y+yy,z+zz) == 2 then
            level:SetBlock(nil,x,y,z,2)
            return;
          end
        end
      end
    end
  end
}
server.InitializedEvent:Add(function()
  Grassgrowth.Start(server.level)
end)
