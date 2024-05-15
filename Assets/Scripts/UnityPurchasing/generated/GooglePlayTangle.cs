// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("QLxwntMAAlc2dbcQ0D91JaCNX/dF93RXRXhzfF/zPfOCeHR0dHB1dvhYu4KsTfgN1JCzFtHXtKMtCd7ztCfYd0jdRvcU4NAXIptXPni7/6/vPMYFjHKm1Ff8MzEiYBSG2vo0wIIRmF4RjbyEOC01RbYKanBKYiWe0Lr91fKkRk23e5IEaTvqL0UfK0CRrkkTLvsMp/L/OVqY9z3adbu5lcdE7RSvPiysDswbQMYualkBJeDR5fsVUSNLrFvS4XJRyHsr5K30SjyNCTStblnBWyTEuMtkscAhTdLrufd0enVF93R/d/d0dHWicuoSI5U1kGgE0IiaF6OfJGRL9ZCfMD62Rz79dmUYHFlbMFhFz25WSScp2eqP3gEmDNhtHnXgKnd2dHV0");
        private static int[] order = new int[] { 9,9,11,3,13,13,10,8,11,13,12,13,13,13,14 };
        private static int key = 117;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
