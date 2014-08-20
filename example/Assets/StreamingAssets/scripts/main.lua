-- 
-- UIEventListener = luanet.import_type('UIEventListener')

-- function OnClick(obj)
-- 	local label = obj:GetComponent("UILabel");
-- 	label.text = "Csharp2";

-- 	print("unity lua click")
-- end

-- --local label3 = GameObject.Find("Label2");
-- --UIEventListener.Get(label3).onClick = OnClick;

-- TB = {TB1 = {}};

-- function TB.TB1.Start(gameObject)
-- 	local label3 = gameObject.transform:FindChild("Label2").gameObject;
-- 	UIEventListener.Get(label3).onClick = OnClick;

-- 	print("unity lua function")
-- end


-- print("unity lua do file\t", os.date())

Lib = Import("scripts/lib.lua");
GameObject = luanet.import_type("UnityEngine.GameObject")


function Start(gameObject)

	print("unity lua do main file\t", gameObject.name);

		local tb = {a = 8, b = {ss = 5}};
	Lib.Tree(tb);


	local name = "Test";
	local test = GameObject(name);
	local getTest = GameObject.Find(name);

	assert(test == getTest);

	local callbackTest = test:AddComponent("CallbackTest");
	callbackTest.OnStart = function (gameObject)
		print("callback sucess!");
		return gameObject.Name == name;
	end

	return true, "acai", "fk liuzhibiao"
end