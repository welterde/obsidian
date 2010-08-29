inlava = {}
server.PlayerLoginEvent:Add(function(player)
  player.MoveEvent:Add(function()
    local level = player.Level
    local x = math.floor(player.Position.X/32)
    local y = math.floor(player.Position.Y/32)-1
    local z = math.floor(player.Position.Z/32)
    if x<0 or y<0 or z<0 or x>=level.Width or y>=level.Depth or z>=level.Height then return end
    local block = level:GetBlock(x,y,z)
    if block == 10 or block == 11 then
      if not inlava[player] then
        Message("&c*"..player.Group.Prefix..player.Name..player.Group.Postfix.."&c died."):Send(level)
        player:Teleport(level.Spawn)
        inlava[player] = true
      end
    else inlava[player] = nil end
  end)
end)
