namespace FunctionAssigment {
    internal class Program {

        /// <summary>
        /// Kysy käyttäjältä nimi.
        /// </summary>
        /// <returns>
        /// Palauttaa string-tyyppisen nimen.
        /// </returns>
        static string KysyNimi() {

            // Ask for name and ensure it is not empty
            while(true) {
                Console.Write("Enter your name: ");
                string? name = Console.ReadLine();
                if(!string.IsNullOrWhiteSpace(name))
                    return name.Trim();
                Console.WriteLine("Name cannot be empty.");
            }

        }

        /// <summary>
        ///  Kysy käyttäjän iän.
        /// </summary>
        /// <returns>
        ///  Palauttaa int-tyyppisen iän.
        /// </returns>
        static int KysyIka() {

            while(true) {
                Console.WriteLine("Enter your age: ");
                string? input = Console.ReadLine();

                if(int.TryParse(input, out int age) && age > 0)
                    return age;

                Console.WriteLine("Please enter a positive integer.");
                
            }

        }


        /// <summary>
        /// Tulosta käyttäjän nimi ja ikä.
        /// </summary>
        /// <param name="nimi">String-tyyppinen nimi.</param>
        /// <param name="ika">Int-tyyppinen ikä</param>
        static void TulostaNimiJaIka(string nimi, int ika) => Console.WriteLine(
                                                         $"Your name is {nimi} and your age is {ika}.");

        /// <summary>
        /// Tarkistaa onko ikä suurempi kuin 18.
        /// </summary>
        /// <param name="ika"></param>
        /// <returns>
        /// Palauttaa true, kun ikä on 18 tai suurempi,
        /// ja false, kun ikä on pienempi kuin 18.
        /// </returns>
        static bool TarkistaTaysiIkainen(int ika) => ika >= 18;

        /// <summary>
        /// Vertaa kaksi string-tyyppistä.
        ///
        /// Vertaa merkkijonot suoraan tai ilman kirjainkoon merkitystä. 
        /// </summary>
        /// <param name="nimi">String-tyyppinen nimi.</param>
        /// <param name="compareTo">String-tyyppinen verattava merkkijono.</param>
        static void VertaaNimea(string nimi, string compareTo) {
            if(nimi.Equals(compareTo, StringComparison.OrdinalIgnoreCase)) {
                Console.WriteLine($"Your name matches \"{compareTo}\" (case-sensitive).");
            }

            if(nimi.Equals(compareTo)) {
                Console.WriteLine($"Your name is exactly \"{compareTo}\" (case-sensitive).");
            }

        }

        static void Main() {
            string name = KysyNimi();
            int age = KysyIka();

            TulostaNimiJaIka(name, age);
            bool isFullAge = TarkistaTaysiIkainen(ika);

            if(isFullAge) {
                Console.WriteLine("You are an adult.");
            } else {
                Console.WriteLine("You are not an adult.");
            }

            VertaaNimea(name, "Matti");
        }
    }
}
