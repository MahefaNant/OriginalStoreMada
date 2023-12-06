-- insert into utilisateur values (default, 'admin1','xxx','admin1@gmail.com',crypt('admin1', gen_salt('bf')),'admin');

insert into client values (default, 'mahefanant@gmail.com', 'Mahefa', 'xxx', 'Lotsdasxxx', crypt('mahefa', gen_salt('bf')));

INSERT INTO admin
VALUES (default,'admin@gmail.com', 'admin' ,'xxx', 'adresse xxx', crypt('admin', gen_salt('bf')), 5);

SELECT * FROM admin WHERE code = crypt('admin', code);
SELECT * FROM client WHERE code = crypt('nant', code);

INSERT INTO client (mail, nom,prenom,adresse, code)
VALUES ('client1@gmail.com', 'client1', 'xxx', 'Lotsdasxxx', crypt('client1', '$2a$12$SALT'));

INSERT INTO type_produit (nom) VALUES ('homme') , ('femme') , ('mixte');

----------------ONESADSd 

select 2 + 2;

-- insert into adresse_livraisondevis values (default,10,'s', 'a', 15 , 12,null , null, 't' , 20,8,1,null,1);
