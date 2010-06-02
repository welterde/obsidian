Command("abort","","Cancels an action.",
  function(command,player,message)
    if message ~= "" then
      Message("&eSyntax: "..command.syntax):Send(player)
      return
    end
    local info = PlayerNextBlockInfo(player)
    if not info then
      Message("&eThere is no action to abort."):Send(player)
      return
    end
    Message("&e"..info.." aborted."):Send(player)
    PlayerAbortNextBlock(player)
  end
)
