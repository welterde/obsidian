Command("save","[name]","Saves the level.",
  function(command,player,message)
    if message == "" then message = config.level.name end
    if not message then
      Message("&eNo main level specified."):Send(player)
      return
    end
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
