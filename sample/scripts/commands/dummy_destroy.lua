Command("dummy destroy","<name>","Destroys a dummy.",
  function(command,player,message)
    if message == "" then
      Message("&eSyntax: "..command.syntax):Send(player)
      return
    end
    local dummy = nil
    for i,v in ipairs(levelDummies) do
      if v.Name:lower() == message:lower() then
        if dummy then
          if v.Position:DistanceTo(player.Position) <
             dummy.Position:DistanceTo(player.Position) then
            dummy = v
          end
        else dummy = v end
      end
    end
    if dummy then
      Message("&eDummy '"..dummy.Name.."' destroyed."):Send(player)
      table.removevalue(levelDummies,dummy)
      dummy.Visible = false
    else
      Message("&eThere is no dummy '"..message.."'."):Send(player)
    end
  end
)
