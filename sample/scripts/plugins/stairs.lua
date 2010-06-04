Stairs = {
  running = false,
  Start = function(level)
    if Stairs.running then error("Already running.") end
    Stairs.level = level
    Stairs.handler = level.BlockEvent:Add(Stairs.Block)
    Stairs.running = true
  end,
  Stop = function()
    if not Stairs.running then error("Not running.") end
    Stairs.level.BlockEvent:Remove(Stairs.handler)
    Stairs.running = false
  end,
  Block = function(level,args)
    if args.Type == 44 and args.Y ~= 0 and
       level:GetBlock(args.X,args.Y-1,args.Z) == 44 then
      args.Abort = true
      level:SetBlock(args.Origin,args.X,args.Y,args.Z,0)
      level:SetBlock(args.Origin,args.X,args.Y-1,args.Z,43)
    end
  end
}
server.InitializedEvent:Add(function()
  Stairs.Start(server.level)
end)
