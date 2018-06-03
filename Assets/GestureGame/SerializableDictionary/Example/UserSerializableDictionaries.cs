﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class StringStringDictionary : SerializableDictionary<string, string> {}

[Serializable]
public class ObjectColorDictionary : SerializableDictionary<UnityEngine.Object, Color> {}

[Serializable]
public class AreaStatusColorDictionary : SerializableDictionary<AreaStatus, Color> { }

[Serializable]
public class StringImageDictionary : SerializableDictionary<string, Image> { }
