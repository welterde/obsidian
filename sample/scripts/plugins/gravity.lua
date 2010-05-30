Gravity = {
  running = false,
  Start = function(level)
    if Gravity.running then error("Already running.") end
    Gravity.level = level
    Gravity.counter = 0
    Gravity.handler = level.BlockEvent:Add(Gravity.Block)
    Gravity.running = true
  end,
  Stop = function()
    if not Gravity.running then error("Not running.") end
    Gravity.level.BlockEvent:Remove(Gravity.handler)
    Gravity.running = false
  end,
  Block = function(level,args)
    if (args.Type == 12 or args.Type == 13) and args.Y ~= 0 then
      local below = level:GetBlock(args.X,args.Y-1,args.Z)
      below = Blocktype.FindById(below)
      if not below.Solid then
        level:SetBlockData(args.X,args.Y,args.Z,Gravity.counter)
        server.Queue:Add(140,Gravity.Fall,{args.Origin,args.X,args.Y,args.Z,Gravity.counter})
        Gravity.counter = (Gravity.counter + 1) % 256
      end
    else
      below = Blocktype.FindById(args.Type)
      if not below.Solid and args.Y ~= level.Depth-1 then
        local above = level:GetBlock(args.X,args.Y+1,args.Z)
        if above == 12 or above == 13 then
          level:SetBlockData(args.X,args.Y+1,args.Z,Gravity.counter)
          server.Queue:Add(140,Gravity.Fall,{args.Origin,args.X,args.Y+1,args.Z,Gravity.counter})
          Gravity.counter = (Gravity.counter + 1) % 256
        end
      end
    end
  end,
  Fall = function(args)
    if not Gravity.running then return end
    local player,x,y,z,counter = unpack(args)
    local above = Gravity.level:GetBlock(x,y,z)
    if above ~= 12 and above ~= 13 then return end
    if Gravity.level:GetBlockData(x,y,z) ~= counter then return end
    if Blocktype.FindById(Gravity.level:GetBlock(x,y-1,z)).Solid then return end
    player.Level:SetBlock(player,x,y,z,0)
    player.Level:SetBlock(player,x,y-1,z,above)
  end
}
