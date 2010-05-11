Command("create portalbutton","","Creates a new portal button.",
  function(command,player,message)
    if message ~= "" then
      Message("&eSyntax: "..command.syntax):Send(player)
	  return
    end
    playerNextBlock[player] = { handle = player.BlockEvent:Add(PortalbuttonCreateFirst), name = "Portal button creation" }
	Message("&eSelect the portal."):Send(player)
  end
)

function PortalbuttonCreateFirst(player,args)
  player.BlockEvent:Remove(playerNextBlock[player].handle)
  local regions = player.Level:RegionsAt(args.X,args.Y,args.Z)
  local portal = nil
  for i = 0,regions.Count-1 do
    local region = regions[i]
    if Is(region,"Portal") then
      portal = region
    end
  end
  if portal == nil then
    Message("&eThere is no portal at that position."):Send(player)
    return
  end
  Message("&ePlace the button."):Send(player)
  playerNextBlock[player] = { handle = player.BlockEvent:Add(PortalbuttonCreateSecond), name = "Portal button creation", ["portal"] = portal }
  args.abort = true
end

function PortalbuttonCreateSecond(player,second)
  
end