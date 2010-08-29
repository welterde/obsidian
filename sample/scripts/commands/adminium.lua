Command("adminium","[<player>]","Changes if a player can destroy adminium.",
  function(command,player,message)
    local target = player
    if message ~= "" then
      target = FindPlayer(message)
      if target == nil then
        Message("&eThere is no player '"..message.."'."):Send(player)
        return
      end
    end
    local text = " now can"
    target.DestroyAdminium = not target.DestroyAdminium
    if not target.DestroyAdminium then text = text.."'t" end
    text = text.." destroy adminium."
    if player ~= target then
      Message("&e"..target.Name..text):Send(player)
    end
    Message("&eYou"..text):Send(target)
  end
)
