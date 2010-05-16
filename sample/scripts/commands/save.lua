Command("save","[name]","Saves the level.",
  function(command,player,message)
    if message == "" then message = server.MainLevel end
    local status,err = pcall(SaveLevel,message)
    if status then
      Message("&eLevel saved as '"..message.."'."):Send(server)
    else
      if type(err) == "userdata" and Is(err.Message,"ArgumentException") then
        Message("&eOnly alphanumerical characters allowed."):Send(player)
      else error(err) end
    end
  end
)

function SaveLevel(filename)
  LevelSave(server.Level)
  server.Level:Save(filename)
end