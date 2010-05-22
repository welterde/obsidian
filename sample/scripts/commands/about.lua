Command("about","","Shows information about a block.",
  function(command,player,message)
    if message ~= "" then
      Message("&eSyntax: "..command.syntax):Send(player)
	  return
    end
    playerNextBlock[player] = { handle = player.BlockEvent:Add(AboutBlock), name = "Showing block information" }
	Message("&eSelect a block."):Send(player)
  end
)

function AboutBlock(player,args)
  player.BlockEvent:Remove(playerNextBlock[player].handle)
  playerNextBlock[player] = nil
  local x,y,z = args.x,args.y,args.z
  local t = player.Level:GetBlock(x,y,z)
  local b = Blocktype.FindById(t)
  Message("&eBlock at "..x..","..y..","..z.." is "..b.Name.." ("..t..")"):Send(player)
  args.Abort = true
end
