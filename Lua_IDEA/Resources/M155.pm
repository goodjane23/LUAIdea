function m155()
	local XWidth = 70
	local YWidth = 50
	local SafeZ  = 3
	local ProbeZ = -3
	local StepX  = 15
	local StepY  = 15
	local Feed   = 50
	local TipHeight = 0
	local ProbeFilename = "C:\\temp\\probe.txt"
	
	PushCurrentDistanceMode()
	PushCurrentMotionMode()
	
	if (IsProbingPinConfigured()) then
		-- open the file
		file, msg = io.open(ProbeFilename, "w")
		
		if (file == nil) then
			DisplayMessage("Could not open probe output file ("..msg..")")
			Stop()
			return
		end
		
		ExecuteMDI("F "..Feed)
		ExecuteMDI("G90 G38.2 Z-100")
		
		-- set the current location to 0,0,0
		ExecuteMDI("G92 X0Y0Z0")
		ExecuteMDI("G0 Z"..SafeZ)
		
		local direction = 0
		for y = 0, YWidth, StepY do
			if (direction == 1) then
				direction = 0
			else
				direction = 1
			end
			
			for x = 0, XWidth, StepX do
				if (direction == 1) then
					ExecuteMDI("G0 X"..x.." Y"..y.." Z"..SafeZ)
				else
					ExecuteMDI("G0 X"..(XWidth - x).." Y"..y.." Z"..SafeZ)
				end
				
				ExecuteMDI("G38.2 Z"..ProbeZ)
				LogCurrentPos(TipHeight)
				ExecuteMDI("G0 Z"..SafeZ)
			end
		end
		
		if (direction == 1) then
			ExecuteMDI("G0 X"..XWidth.." Y"..YWidth.." Z"..SafeZ)
		else
			ExecuteMDI("G0 X".."0".." Y"..YWidth.." Z"..SafeZ)
		end
		
		local HighZ = 5
		ExecuteMDI("G0 Z"..HighZ)
		ExecuteMDI("G0 X0Y0")
		
		file:close()
	else
		DisplayMessage("Probe input is not configured")
		return
	end
end

function LogCurrentPos(tipHeight)
	local CurrX = AxisGetPos(Axis.X)
	local CurrY = AxisGetPos(Axis.Y)
	local CurrZ = AxisGetPos(Axis.Z)
	
	local fmt = "%.5f"
	file:write(string.format(fmt, CurrX)..","..string.format(fmt, CurrY)..","..string.format(fmt, CurrZ - tipHeight), "\n")
end