Command("save","[name]","Saves the level.",
  function(command,player,message)
    if message == "" then message = server.MainLevel end
    local status,err = pcall(SaveLevel,message)
    if status then
      Message("&eLevel saved as '"..message.."'."):Send(server)
    else
      Message("&eCould not save level. Invalid filename?"):Send(player)
    end
  end
)

function SaveLevel(message)
  server.Level:Save(message)
end