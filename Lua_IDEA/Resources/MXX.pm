function m155()
	local outputs_count = 8
                for i = 0, outputs_count  do 
                    if (PinGetState(Outputs.UserOutput_0 + i)) then
                        DisplayMessage("Output_"..i.." ON")
                    else
                        DisplayMessage("Output_"..i.." OFF")
                    end
                end
end