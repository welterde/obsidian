Command("abort","","Cancels an action.",
  function(command,player,message)
    if message ~= "" then
      Message("&eSyntax: "..command.syntax):Send(player)
	  return
    end
	if playerNextBlock[player] == nil then
	  Message("&eThere is no action to abort."):Send(player)
	  return
	end
    player.BlockEvent:Remove(playerNextBlock[player].handle)
	Message("&e"..playerNextBlock[player].name.." aborted."):Send(player)
	playerNextBlock[player] = nil
  end
)