Command("destroy","","Destroys stuff.",
  function(command,player,message)
    if message ~= "" then
      Message("&eSyntax: "..command.syntax):Send(player)
	  return
    end
	Message("&eSelect something."):Send(player)
    PlayerSetNextBlock(player,DestroySomething,"Object destroying")
  end
)

function DestroySomething(player,args)
  local regions = player.Level:RegionsAt(args.X,args.Y,args.Z)
  args.Abort = true
  if regions.Count == 0 then
    Message("&eThere is nothing at this position."):Send(player)
    return
  end
  for i = 0,regions.Count-1 do
    local region = regions[i]
    if Is(region.Tag,"obsidian.World.Objects.Portal") then
      table.removevalue(levelPortals,region.Tag)
      Message("&ePortal destroyed."):Send(player)
      region.Tag:Destroy()
    else
      Message("&eRegion destroyed."):Send(player)
      region:Destroy()
    end
  end
end
